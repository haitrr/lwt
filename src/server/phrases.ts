import 'server-only';
import { and, asc, eq, gt, gte, inArray, lte, ne, or } from 'drizzle-orm';
import { getDb, schema, type Tx } from './db';
import { UserError } from './errors';
import { addLog } from './logs';
import { getLanguage, termKey, type Language } from './languages';
import { buildPhraseSet, matchPhrase, mergePhrases, phraseKey, type PhraseToken } from './languages/phrases';
import { resolveTermIds } from './processing';
import type { Session } from './auth/session';
import { isLearningLevel } from '@/lib/types';

// Phrase terms join tokens the splitter cut apart ("John Maynard Keynes", "GPT-6"). Whether words form a phrase can depend on
// where they appear, so the user joins or splits either one occurrence or all of them. An auto-join phrase is merged
// in every text of its language, texts split later included (src/server/processing.ts); the others only where joined.

const { texts, textTerms, terms } = schema;

// Content is varchar(95).
const MAX_PHRASE_LENGTH = 95;
// Occurrences checked per query when merging.
const BATCH = 200;

type Row = { id: number; index: number; content: string; termId: number | null; termContent: string | null };

function toTokens(rows: Row[], language: Language): PhraseToken[] {
  return rows.map((r) => ({
    text: r.content,
    key: r.termId == null || r.termContent == null ? null : language.normalize(r.termContent),
  }));
}

function rowColumns() {
  return {
    id: textTerms.id,
    textId: textTerms.textId,
    index: textTerms.index,
    content: textTerms.content,
    termId: textTerms.termId,
    termContent: terms.content,
  };
}

type Owner = { userId: number; languageCode: string };

async function getOwnedText(textId: number, userId: number) {
  const [text] = await getDb()
    .select({ languageCode: texts.languageCode })
    .from(texts)
    .where(and(eq(texts.id, textId), eq(texts.userId, userId)));
  if (!text) throw new UserError('Text not found.', 404);
  return text;
}

async function getOwnedPhrase(termId: number, userId: number) {
  const [term] = await getDb()
    .select({ content: terms.content, languageCode: terms.languageCode, phrase: terms.phrase, autoJoin: terms.autoJoin })
    .from(terms)
    .where(and(eq(terms.id, termId), eq(terms.userId, userId)));
  if (!term) throw new UserError('term not found', 404);
  if (!term.phrase) throw new UserError('This term is not a phrase.');
  return term;
}

/** Keys of the phrases joined everywhere, but `except`. */
async function autoJoinKeys(tx: Tx, owner: Owner, except?: number) {
  const rows = await tx
    .select({ content: terms.content })
    .from(terms)
    .where(
      and(
        eq(terms.userId, owner.userId),
        eq(terms.languageCode, owner.languageCode),
        eq(terms.autoJoin, true),
        except === undefined ? undefined : ne(terms.id, except),
      ),
    );
  return rows.map((r) => r.content);
}

/** The term of the first word in `content`, as the splitter cuts it on its own. */
async function firstWordTermId(tx: Tx, owner: Owner, language: Language, content: string) {
  const key = (await language.split(content)).map((w) => termKey(language, w)).find((k) => k !== null);
  if (key == null) return undefined;
  return (await resolveTermIds(tx, owner, new Set([key]))).get(key);
}

/** Merges rows (consecutive tokens of one text) into one token of term `phraseId`. */
async function mergeRows(tx: Tx, rows: { id: number; content: string }[], phraseId: number) {
  await tx
    .update(textTerms)
    .set({ termId: phraseId, content: rows.map((r) => r.content).join('') })
    .where(eq(textTerms.id, rows[0].id));
  if (rows.length > 1) {
    await tx.delete(textTerms).where(
      inArray(
        textTerms.id,
        rows.slice(1).map((r) => r.id),
      ),
    );
  }
}

/** The tokens from index `from` to `to` of a text and the key of the phrase they spell. */
async function readSelection(session: Session, { textId, from, to }: { textId: number; from: number; to: number }) {
  const text = await getOwnedText(textId, session.userId);
  const language = getLanguage(text.languageCode);
  const owner = { userId: session.userId, languageCode: text.languageCode };
  const rows = await getDb()
    .select(rowColumns())
    .from(textTerms)
    .leftJoin(terms, eq(textTerms.termId, terms.id))
    .where(and(eq(textTerms.textId, textId), gte(textTerms.index, from), lte(textTerms.index, to)))
    .orderBy(asc(textTerms.index));
  const key = checkKey(phraseKey(toTokens(rows, language), language.phraseSeparator));
  return { owner, language, rows, key };
}

/** The phrase term the selected words spell, when the user already made it (joined elsewhere, or split here). */
export async function findPhrase(session: Session, selection: { textId: number; from: number; to: number }) {
  const { owner, key } = await readSelection(session, selection);
  const [term] = await getDb()
    .select({ id: terms.id, meaning: terms.meaning, learningLevel: terms.learningLevel, autoJoin: terms.autoJoin })
    .from(terms)
    .where(and(eq(terms.userId, owner.userId), eq(terms.languageCode, owner.languageCode), eq(terms.content, key)));
  return term ?? null;
}

/**
 * Makes the words from token index `from` to `to` of a text one term, reusing the phrase term if there is one.
 * everywhere also joins them in every other text and makes the phrase auto-join; otherwise only this occurrence joins.
 */
export async function makePhrase(
  session: Session,
  input: { textId: number; from: number; to: number; learningLevel: string; meaning: string; everywhere: boolean },
) {
  if (!isLearningLevel(input.learningLevel)) throw new UserError('Invalid learning level.');
  const { owner, language, rows, key } = await readSelection(session, input);

  return getDb().transaction(async (tx) => {
    const id = await savePhraseTerm(tx, session, owner, key, input);
    await mergeRows(tx, rows, id);
    if (input.everywhere) await joinEverywhere(tx, owner, language, id, key, [rows[0].termId!, rows[0].content]);
    return id;
  });
}

/**
 * Makes `content` (text as written, e.g. from a web page) one term that joins in every text, as makePhrase does with
 * everywhere. Returns the phrase term.
 */
export async function makePhraseEverywhere(
  session: Session,
  input: { languageCode: string; content: string; learningLevel: string; meaning: string },
) {
  if (!isLearningLevel(input.learningLevel)) throw new UserError('Invalid learning level.');
  const language = getLanguage(input.languageCode);
  const owner = { userId: session.userId, languageCode: input.languageCode };
  const content = input.content.trim();
  const words = await language.split(content);
  const key = checkKey(phraseKey(words.map((w) => ({ text: w, key: termKey(language, w) })), language.phraseSeparator));

  const id = await getDb().transaction(async (tx) => {
    const id = await savePhraseTerm(tx, session, owner, key, { ...input, everywhere: true });
    await joinEverywhere(tx, owner, language, id, key, [content]);
    return id;
  });
  return { id, key };
}

function checkKey(key: string | null) {
  if (!key) throw new UserError('A phrase is a word followed by more words, numbers or punctuation, in one paragraph.');
  if ([...key].length > MAX_PHRASE_LENGTH) throw new UserError(`A phrase can be at most ${MAX_PHRASE_LENGTH} characters.`);
  return key;
}

/** Saves the level and meaning of the phrase term `key`, making it when there is none. */
async function savePhraseTerm(
  tx: Tx,
  session: Session,
  owner: Owner,
  key: string,
  input: { learningLevel: string; meaning: string; everywhere: boolean },
) {
  const owned = and(eq(terms.userId, owner.userId), eq(terms.languageCode, owner.languageCode));
  const [existing] = await tx
    .select({ id: terms.id, learningLevel: terms.learningLevel, autoJoin: terms.autoJoin })
    .from(terms)
    .where(and(owned, eq(terms.content, key)));
  const values = {
    meaning: input.meaning,
    learningLevel: input.learningLevel,
    phrase: true,
    autoJoin: input.everywhere || (existing?.autoJoin ?? false),
  };
  if (existing) {
    await tx.update(terms).set(values).where(eq(terms.id, existing.id));
    const snapshot = (learningLevel: string) => ({ Content: key, LanguageCode: owner.languageCode, LearningLevel: learningLevel });
    await addLog(
      tx,
      'termEdited',
      { Before: snapshot(existing.learningLevel), After: snapshot(input.learningLevel) },
      session.userName,
      { type: 'Term', id: existing.id },
    );
    return existing.id;
  }
  const [{ id }] = await tx
    .insert(terms)
    .values({ ...values, content: key, languageCode: owner.languageCode, userId: owner.userId })
    .returning({ id: terms.id });
  await addLog(
    tx,
    'termCreated',
    { LanguageCode: owner.languageCode, Content: key, LearningLevel: input.learningLevel },
    session.userName,
    { type: 'Term', id },
  );
  return id;
}

/** Makes a phrase auto-join and joins it in every text where its words appear apart. */
export async function joinPhraseEverywhere(session: Session, termId: number) {
  const term = await getOwnedPhrase(termId, session.userId);
  const language = getLanguage(term.languageCode);
  const owner = { userId: session.userId, languageCode: term.languageCode };
  await getDb().transaction(async (tx) => {
    await tx.update(terms).set({ autoJoin: true }).where(eq(terms.id, termId));
    const [sample] = await tx
      .select({ content: textTerms.content })
      .from(textTerms)
      .where(eq(textTerms.termId, termId))
      .limit(1);
    await joinEverywhere(tx, owner, language, termId, term.content, sample ? [sample.content] : []);
  });
}

/**
 * Joins each run of separate tokens spelling `key` into one token of term `phraseId`. Runs are looked for where
 * a starting token appears: a term id, or the text of an occurrence whose first word is looked up.
 */
async function joinEverywhere(
  tx: Tx,
  owner: Owner,
  language: Language,
  phraseId: number,
  key: string,
  starts: (number | string)[],
) {
  const startIds = new Set<number>();
  for (const start of starts) {
    const id = typeof start === 'number' ? start : await firstWordTermId(tx, owner, language, start);
    if (id !== undefined && id !== phraseId) startIds.add(id);
  }
  if (startIds.size === 0) return;
  // Occurrences may have more spaces between the tokens than the key, so look a little further.
  const span = key.length * 2 + 8;
  const set = buildPhraseSet([key], language.phraseSeparator);
  const found = await tx
    .select({ textId: textTerms.textId, index: textTerms.index })
    .from(textTerms)
    .where(inArray(textTerms.termId, [...startIds]))
    .orderBy(asc(textTerms.textId), asc(textTerms.index));

  for (let b = 0; b < found.length; b += BATCH) {
    const batch = found.slice(b, b + BATCH);
    const rows = await tx
      .select(rowColumns())
      .from(textTerms)
      .leftJoin(terms, eq(textTerms.termId, terms.id))
      .where(
        or(
          ...batch.map((s) =>
            and(eq(textTerms.textId, s.textId), gte(textTerms.index, s.index), lte(textTerms.index, s.index + span)),
          ),
        ),
      )
      .orderBy(asc(textTerms.textId), asc(textTerms.index));

    for (const [textId, textRows] of Map.groupBy(rows, (r) => r.textId)) {
      const tokens = toTokens(textRows, language);
      let consumed = -1;
      for (const start of batch.filter((s) => s.textId === textId)) {
        const at = textRows.findIndex((r) => r.index === start.index);
        if (at <= consumed) continue;
        const end = matchPhrase(tokens, at, set);
        if (end == null) continue;
        await mergeRows(tx, textRows.slice(at, end + 1), phraseId);
        consumed = end;
      }
    }
  }
}

/**
 * Turns a phrase back into its words: at one occurrence (`at`), where it stays a phrase everywhere else,
 * or everywhere, which deletes the phrase term.
 */
export async function splitPhrase(session: Session, termId: number, at: { textId: number; index: number } | null) {
  const term = await getOwnedPhrase(termId, session.userId);
  const language = getLanguage(term.languageCode);
  const owner = { userId: session.userId, languageCode: term.languageCode };

  await getDb().transaction(async (tx) => {
    // A longer phrase may contain a shorter one ("John Maynard Keynes", "Maynard Keynes"): keep that one joined.
    const set = buildPhraseSet(await autoJoinKeys(tx, owner, termId), language.phraseSeparator);
    const occurrences = await tx
      .select({ id: textTerms.id, textId: textTerms.textId, index: textTerms.index, content: textTerms.content })
      .from(textTerms)
      .where(
        and(
          eq(textTerms.termId, termId),
          at ? and(eq(textTerms.textId, at.textId), eq(textTerms.index, at.index)) : undefined,
        ),
      );
    if (at && occurrences.length === 0) throw new UserError('That phrase is not there any more; reload the text.');

    const resplit = new Set<number>();
    const inserts: { textId: number; index: number; text: string; key: string | null }[] = [];
    for (const occurrence of occurrences) {
      const words = await language.split(occurrence.content);
      const tokens = words.map((w) => ({ text: w, key: termKey(language, w) }));
      const [next] = await tx
        .select({ index: textTerms.index })
        .from(textTerms)
        .where(and(eq(textTerms.textId, occurrence.textId), gt(textTerms.index, occurrence.index)))
        .orderBy(asc(textTerms.index))
        .limit(1);
      if (next && occurrence.index + tokens.length > next.index) {
        // Split alone, the words came out differently than in the text.
        if (at) throw new UserError('This phrase can only be split everywhere.');
        resplit.add(occurrence.textId);
        continue;
      }
      const { merged } = mergePhrases(tokens, 0, tokens.length, set);
      inserts.push(...merged.map((t) => ({ textId: occurrence.textId, index: occurrence.index + t.index, text: t.text, key: t.key })));
    }

    const termIds = await resolveTermIds(tx, owner, new Set(inserts.flatMap((t) => (t.key === null ? [] : [t.key]))));
    await tx.delete(textTerms).where(
      inArray(
        textTerms.id,
        occurrences.map((o) => o.id),
      ),
    );
    if (inserts.length) {
      await tx.insert(textTerms).values(
        inserts.map((t) => ({
          textId: t.textId,
          index: t.index,
          content: t.text,
          termId: t.key === null ? null : (termIds.get(t.key) ?? null),
        })),
      );
    }
    if (resplit.size) {
      // The whole text is split again instead.
      await tx.delete(textTerms).where(inArray(textTerms.textId, [...resplit]));
      await tx.update(texts).set({ termCount: 0, processedTermCount: 0 }).where(inArray(texts.id, [...resplit]));
    }
    if (!at) await tx.delete(terms).where(eq(terms.id, termId));
  });
}

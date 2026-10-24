import 'server-only';
import { and, eq, inArray } from 'drizzle-orm';
import { getDb, schema } from './db';
import { lookUpMeaning } from './dictionary';
import { UserError } from './errors';
import { getLanguage, hasSplitter, termKey, type Language } from './languages';
import { buildPhraseSet, mergePhrases, type PhraseSet } from './languages/phrases';
import { makePhraseEverywhere, splitPhrase } from './phrases';
import { createTerm, editTerm, getTerm } from './terms';
import { getLanguageSettings, getTranslatorSettings } from './users';
import type { Session } from './auth/session';
import type { LearningLevel, TermView } from '@/lib/types';

// Reading any web page with the browser extension: the page's text is split like a text's would be, but nothing is
// stored for it. Words the user has no term for come back without one and only become a term once they set a level.

const { terms } = schema;

/** Most text one request may send; the extension sends a long page in several. */
export const MAX_SEGMENTS = 500;
export const MAX_CHARS = 50_000;

/** A word or phrase in a segment: its UTF-16 offsets there and its term key. */
export type WebToken = { start: number; end: number; key: string };

export type WebTerm = { id: number; learningLevel: LearningLevel; meaning: string; phrase: boolean };

export type WebAnnotation = {
  /** The words of each segment sent, in order; separators are left out. */
  segments: WebToken[][];
  /** The user's terms for those words, by key. A key that is missing has no term yet: it reads as unknown. */
  terms: Record<string, WebTerm>;
};

function requireLanguage(code: unknown): Language {
  if (typeof code !== 'string' || !hasSplitter(code)) throw new UserError('Unsupported language.');
  return getLanguage(code);
}

/**
 * Splits each segment into words, merging the given phrases, with their offsets in the segment.
 * Splitters may drop characters (Japanese drops ASCII spaces), so each word is looked up from where the last ended.
 */
export async function splitSegments(language: Language, segments: string[], phrases: PhraseSet): Promise<WebToken[][]> {
  const result: WebToken[][] = [];
  for (const segment of segments) {
    const words = await language.split(segment);
    const offsets: number[] = [];
    let cursor = 0;
    for (const word of words) {
      const at = segment.indexOf(word, cursor);
      offsets.push(at);
      if (at >= 0) cursor = at + word.length;
    }
    const tokens = words.map((text) => ({ text, key: termKey(language, text) }));
    const { merged } = mergePhrases(tokens, 0, tokens.length, phrases);
    const found: WebToken[] = [];
    merged.forEach((token, i) => {
      const last = (merged[i + 1]?.index ?? tokens.length) - 1;
      const start = offsets[token.index];
      if (token.key === null || start < 0 || offsets[last] < 0) return;
      found.push({ start, end: offsets[last] + words[last].length, key: token.key });
    });
    result.push(found);
  }
  return result;
}

/**
 * Terms compare ignoring case, accents and width (the lwt_ci collation), so a key can match a term spelled a little
 * differently. This folds both sides about the same way to pair them up again.
 */
function fold(content: string) {
  return content.normalize('NFKD').replace(/\p{M}/gu, '').toUpperCase();
}

export async function annotateWebText(session: Session, input: { languageCode?: unknown; segments?: unknown }) {
  const language = requireLanguage(input.languageCode);
  const segments = input.segments;
  if (!Array.isArray(segments) || !segments.every((s) => typeof s === 'string')) {
    throw new UserError('segments must be a list of strings.');
  }
  if (segments.length > MAX_SEGMENTS || segments.reduce((n, s) => n + s.length, 0) > MAX_CHARS) {
    throw new UserError(`Send at most ${MAX_SEGMENTS} segments and ${MAX_CHARS} characters at a time.`);
  }

  const owned = and(eq(terms.userId, session.userId), eq(terms.languageCode, language.code));
  const db = getDb();
  const phraseRows = await db
    .select({ content: terms.content })
    .from(terms)
    .where(and(owned, eq(terms.autoJoin, true)));
  const phrases = buildPhraseSet(
    phraseRows.map((r) => r.content),
    language.phraseSeparator,
  );
  const tokens = await splitSegments(language, segments, phrases);

  const keys = [...new Set(tokens.flat().map((t) => t.key))];
  const byFold = new Map<string, WebTerm>();
  for (let i = 0; i < keys.length; i += 1000) {
    const rows = await db
      .select({ id: terms.id, content: terms.content, learningLevel: terms.learningLevel, meaning: terms.meaning, phrase: terms.phrase })
      .from(terms)
      .where(and(owned, inArray(terms.content, keys.slice(i, i + 1000))));
    for (const { content, id, learningLevel, meaning, phrase } of rows) {
      byFold.set(fold(content), { id, learningLevel: learningLevel as LearningLevel, meaning, phrase });
    }
  }
  const found: Record<string, WebTerm> = {};
  for (const key of keys) {
    const term = byFold.get(fold(key));
    if (term) found[key] = term;
  }
  return { segments: tokens, terms: found } satisfies WebAnnotation;
}

/**
 * Sets a word's level and meaning: the term with `termId`, or else the user's term for `word`, made when it has none.
 */
export async function saveWebTerm(
  session: Session,
  input: { termId?: unknown; languageCode?: unknown; word?: unknown; learningLevel?: unknown; meaning?: unknown },
): Promise<TermView> {
  const learningLevel = String(input.learningLevel ?? '');
  const meaning = typeof input.meaning === 'string' ? input.meaning : '';
  let termId = typeof input.termId === 'number' ? input.termId : null;
  if (termId === null) {
    const language = requireLanguage(input.languageCode);
    const key = typeof input.word === 'string' ? termKey(language, input.word.trim()) : null;
    if (!key) throw new UserError('That is not a word that can be saved.');
    const [existing] = await getDb()
      .select({ id: terms.id })
      .from(terms)
      .where(and(eq(terms.userId, session.userId), eq(terms.languageCode, language.code), eq(terms.content, key)));
    if (!existing) {
      const id = await createTerm(session, { content: key, meaning, learningLevel, languageCode: language.code });
      return getTerm(id, session.userId);
    }
    termId = existing.id;
  }
  await editTerm(session, termId, { learningLevel, meaning });
  return getTerm(termId, session.userId);
}

export type WebLookup = { status: 'found'; meaning: string } | { status: 'missing' | 'unavailable' | 'no-dictionary' };

/** A dictionary meaning for a word, in the dictionary language the user chose for its language. */
export async function lookUpWebWord(session: Session, languageCode: string, word: string): Promise<WebLookup> {
  requireLanguage(languageCode);
  if (!word.trim()) throw new UserError('word is required.');
  const settings = await getLanguageSettings(session.userId);
  const dictionaryLanguage = settings.find((s) => s.languageCode === languageCode)?.dictionaryLanguageCode;
  if (!dictionaryLanguage) return { status: 'no-dictionary' };
  const meaning = await lookUpMeaning(word, languageCode, dictionaryLanguage, await getTranslatorSettings(session.userId));
  return meaning ? { status: 'found', meaning } : { status: meaning === null ? 'missing' : 'unavailable' };
}

/**
 * Makes text the user selected on a page ("GPT-6") one term, joined wherever it appears: on pages and in their texts.
 * The page then needs annotating again to show it.
 */
export async function makeWebPhrase(
  session: Session,
  input: { languageCode?: unknown; text?: unknown; learningLevel?: unknown; meaning?: unknown },
) {
  const language = requireLanguage(input.languageCode);
  const text = typeof input.text === 'string' ? input.text : '';
  if (!text.trim() || text.length > 500) throw new UserError('Select a few words to join.');
  return makePhraseEverywhere(session, {
    languageCode: language.code,
    content: text,
    learningLevel: String(input.learningLevel ?? ''),
    meaning: typeof input.meaning === 'string' ? input.meaning.trim() : '',
  });
}

/** Splits a phrase back into its words everywhere, deleting the phrase term. */
export async function splitWebPhrase(session: Session, input: { termId?: unknown }) {
  if (typeof input.termId !== 'number') throw new UserError('termId is required.');
  await splitPhrase(session, input.termId, null);
  return { ok: true };
}

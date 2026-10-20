import { and, asc, eq, inArray, lt, or, sql } from 'drizzle-orm';
import { getDb, schema, type Tx } from './db';
import { getLanguage, type Language } from './languages';
import { LearningLevel } from '@/lib/types';

// Background splitting of texts into text_terms, ported from the .NET TextTermProcessor.
// A text is pending while ProcessedTermCount < TermCount, or both are 0 (new or just edited).
// Each step handles one text and at most CHUNK_SIZE tokens, so long books are spread over many steps.

const { texts, terms, textTerms } = schema;

const CHUNK_SIZE = 1000;
const IDLE_DELAY_MS = 1000;
const FAILURE_BACKOFF_MS = 60_000;

type PendingText = {
  id: number;
  userId: number;
  content: string;
  languageCode: string;
  termCount: number;
  processedTermCount: number;
  lastModifiedDate: Date;
};

// Splitting a whole book on every chunk is wasteful, so keep the last few results.
const splitCache = new Map<string, string[]>();

async function splitText(text: PendingText, language: Language) {
  const key = `${text.id}:${text.lastModifiedDate.getTime()}`;
  let words = splitCache.get(key);
  if (!words) {
    words = await language.split(text.content);
    splitCache.set(key, words);
    if (splitCache.size > 8) splitCache.delete(splitCache.keys().next().value!);
  }
  return words;
}

// Texts that just failed are skipped for a while so one bad text can't block the queue.
const failedUntil = new Map<number, number>();

async function setCounts(tx: Tx, id: number, termCount: number, processedTermCount: number) {
  // Keep LastModifiedDate as is: it identifies the content version in splitCache.
  await tx
    .update(texts)
    .set({ termCount, processedTermCount, lastModifiedDate: sql`${texts.lastModifiedDate}` })
    .where(eq(texts.id, id));
}

/**
 * Maps each normalized word to its term id, creating "unknown" terms for new words.
 * terms.Content is compared case- and accent-insensitively by MySQL, so a word can match a
 * term stored with different spelling; those are resolved one by one.
 */
async function resolveTermIds(tx: Tx, text: PendingText, words: Set<string>) {
  const ids = new Map<string, number>();
  if (words.size === 0) return ids;
  const owned = and(eq(terms.userId, text.userId), eq(terms.languageCode, text.languageCode));
  const load = async (contents: string[]) => {
    const rows = await tx
      .select({ id: terms.id, content: terms.content })
      .from(terms)
      .where(and(owned, inArray(terms.content, contents)));
    for (const r of rows) ids.set(r.content, r.id);
  };

  await load([...words]);
  const missing = [...words].filter((w) => !ids.has(w));
  if (missing.length) {
    await tx
      .insert(terms)
      .ignore()
      .values(
        missing.map((content) => ({
          content,
          languageCode: text.languageCode,
          userId: text.userId,
          learningLevel: LearningLevel.Unknown,
          meaning: '',
        })),
      );
    await load(missing);
  }
  for (const word of missing.filter((w) => !ids.has(w))) {
    const [row] = await tx
      .select({ id: terms.id })
      .from(terms)
      .where(and(owned, eq(terms.content, word)))
      .limit(1);
    if (row) ids.set(word, row.id);
  }
  return ids;
}

async function processText(tx: Tx, text: PendingText) {
  const language = getLanguage(text.languageCode);

  if (text.termCount === 0) {
    // New or edited text: drop tokens from a previous version and record the new size.
    await tx.delete(textTerms).where(eq(textTerms.textId, text.id));
    const words = await splitText(text, language);
    await setCounts(tx, text.id, words.length, 0);
    return;
  }

  const words = await splitText(text, language);
  if (words.length !== text.termCount) {
    // The splitter changed since this text was started; start over.
    await setCounts(tx, text.id, 0, 0);
    return;
  }

  const from = text.processedTermCount;
  const chunk = words.slice(from, from + CHUNK_SIZE);
  const normalized = chunk.map((w) => (language.shouldSkip(w) ? null : language.normalize(w)));
  const termIds = await resolveTermIds(tx, text, new Set(normalized.filter((w): w is string => w !== null)));

  await tx.insert(textTerms).values(
    chunk.map((content, i) => ({
      textId: text.id,
      termId: normalized[i] === null ? null : (termIds.get(normalized[i]!) ?? null),
      content,
      index: from + i,
    })),
  );
  await setCounts(tx, text.id, text.termCount, from + chunk.length);
}

/** Runs one step of work. Returns false when nothing is pending. */
export async function processNextText(): Promise<boolean> {
  const now = Date.now();
  const skip = [...failedUntil].filter(([, until]) => until > now).map(([id]) => id);

  let picked: number | undefined;
  try {
    return await getDb().transaction(async (tx) => {
      const pending = or(lt(texts.processedTermCount, texts.termCount), eq(texts.processedTermCount, 0));
      const [text] = await tx
        .select({
          id: texts.id,
          userId: texts.userId,
          content: texts.content,
          languageCode: texts.languageCode,
          termCount: texts.termCount,
          processedTermCount: texts.processedTermCount,
          lastModifiedDate: texts.lastModifiedDate,
        })
        .from(texts)
        .where(skip.length ? and(pending, sql`${texts.id} not in (${sql.join(skip, sql`, `)})`) : pending)
        .orderBy(asc(texts.id))
        .limit(1)
        // Several app instances can run this loop; each takes a different text.
        .for('update', { skipLocked: true });
      if (!text) return false;
      picked = text.id;
      await processText(tx, text);
      return true;
    });
  } catch (e) {
    if (picked === undefined) throw e;
    failedUntil.set(picked, Date.now() + FAILURE_BACKOFF_MS);
    console.error(`Failed to process text ${picked}`, e);
    return true;
  }
}

declare global {
  var lwtProcessingStarted: boolean | undefined;
}

export function startTextProcessing() {
  if (globalThis.lwtProcessingStarted || !process.env.DATABASE_URL) return;
  globalThis.lwtProcessingStarted = true;
  const loop = async () => {
    let busy = false;
    try {
      busy = await processNextText();
    } catch (e) {
      console.error('Text processing failed', e);
    }
    setTimeout(loop, busy ? 0 : IDLE_DELAY_MS);
  };
  void loop();
}

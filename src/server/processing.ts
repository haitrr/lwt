import { and, asc, eq, inArray, isNull, lt, or, sql } from 'drizzle-orm';
import { getDb, schema, type Tx } from './db';
import { getLanguage, termKey, type Language } from './languages';
import { LearningLevel } from '@/lib/types';

// Background splitting of texts into text_terms, ported from the .NET TextTermProcessor.
// A text is pending while ProcessedTermCount < TermCount, or both are 0 (new or just edited),
// unless it has failed MAX_ATTEMPTS times in a row (failed_at is set; editing clears it).
// Each step handles one text and at most CHUNK_SIZE tokens, so long books are spread over many steps.

const { texts, terms, textTerms } = schema;

const CHUNK_SIZE = 1000;
const IDLE_DELAY_MS = 1000;
const FAILURE_BACKOFF_MS = 60_000;
const MAX_ATTEMPTS = 3;

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
// A few retries cover transient errors (deadlocks, dropped connections) before giving up.
const failures = new Map<number, { attempts: number; until: number }>();

async function recordFailure({ id, lastModifiedDate }: PendingText, error: unknown) {
  const attempts = (failures.get(id)?.attempts ?? 0) + 1;
  failures.set(id, { attempts, until: Date.now() + FAILURE_BACKOFF_MS });
  console.error(`Failed to process text ${id} (attempt ${attempts})`, error);
  if (attempts < MAX_ATTEMPTS) return;
  const message = error instanceof Error ? error.message : String(error);
  await getDb()
    .update(texts)
    .set({ processingError: message.slice(0, 1000), failedAt: new Date(), lastModifiedDate: sql`${texts.lastModifiedDate}` })
    // An edit in the meantime gets a fresh start.
    .where(and(eq(texts.id, id), eq(texts.lastModifiedDate, lastModifiedDate)));
  failures.delete(id);
}

async function setCounts(tx: Tx, id: number, termCount: number, processedTermCount: number) {
  // Keep LastModifiedDate as is: it identifies the content version in splitCache.
  await tx
    .update(texts)
    .set({ termCount, processedTermCount, lastModifiedDate: sql`${texts.lastModifiedDate}` })
    .where(eq(texts.id, id));
}

/**
 * Maps each normalized word to its term id, creating "unknown" terms for new words.
 * terms.content is compared case- and accent-insensitively (lwt_ci), so a word can match a
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
  // Sorted so concurrent transactions take the unique-index locks in the same order and can't deadlock.
  const missing = [...words].filter((w) => !ids.has(w)).sort();
  if (missing.length) {
    await tx
      .insert(terms)
      .values(
        missing.map((content) => ({
          content,
          languageCode: text.languageCode,
          userId: text.userId,
          learningLevel: LearningLevel.Unknown,
          meaning: '',
        })),
      )
      .onConflictDoNothing();
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
  const normalized = chunk.map((w) => termKey(language, w));
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
  const skip = [...failures].filter(([, f]) => f.until > now).map(([id]) => id);

  let picked: PendingText | undefined;
  try {
    const result = await getDb().transaction(async (tx) => {
      const pending = and(
        or(lt(texts.processedTermCount, texts.termCount), eq(texts.processedTermCount, 0)),
        isNull(texts.failedAt),
      );
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
      picked = text;
      await processText(tx, text);
      return true;
    });
    if (picked) failures.delete(picked.id);
    return result;
  } catch (e) {
    if (!picked) throw e;
    await recordFailure(picked, e);
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

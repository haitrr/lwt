import 'server-only';
import { and, asc, count, desc, eq, gte, ilike, inArray, isNotNull, lte } from 'drizzle-orm';
import { getDb, schema } from './db';
import { UserError } from './errors';
import { addLog } from './logs';
import { isLanguageCode } from '@/lib/languages';
import type { Session } from './auth/session';
import {
  LearningLevel,
  type TermCounts,
  type TextEditDetail,
  type TextItem,
  type TextList,
  type TextReadInfo,
  type TextTermView,
} from '@/lib/types';

const { texts, textTerms, terms, logs } = schema;

export type TextFilter = { title?: string; languageCode?: string };
export type TextInput = { title: string; languageCode: string; content: string };

function filterConditions(userId: number, filter: TextFilter) {
  const conditions = [eq(texts.userId, userId)];
  if (filter.languageCode) conditions.push(eq(texts.languageCode, filter.languageCode));
  if (filter.title) {
    conditions.push(ilike(texts.title, `%${filter.title.replace(/[\\%_]/g, '\\$&')}%`));
  }
  return and(...conditions);
}

const listColumns = {
  id: texts.id,
  title: texts.title,
  languageCode: texts.languageCode,
  termCount: texts.termCount,
  processedTermCount: texts.processedTermCount,
  bookmark: texts.bookmark,
  createdDate: texts.createdDate,
};

function toTextItem({ createdDate, ...t }: { createdDate: Date } & Omit<TextItem, 'createdAt'>): TextItem {
  return { ...t, createdAt: createdDate.toISOString() };
}

function validate(input: TextInput) {
  if (!isLanguageCode(input.languageCode)) throw new UserError('Unsupported language.');
  if (!input.title.trim()) throw new UserError("'Title' must not be empty.");
  if (!input.content.trim()) throw new UserError("'Content' must not be empty.");
}

async function getOwnedText(id: number, userId: number) {
  const [text] = await getDb()
    .select({ id: texts.id, userId: texts.userId, title: texts.title, languageCode: texts.languageCode, content: texts.content })
    .from(texts)
    .where(eq(texts.id, id));
  if (!text) throw new UserError('Text not found.', 404);
  if (text.userId !== userId) throw new UserError("You don't have permission to access this text.", 403);
  return text;
}

/** Most recently read first, like the old list. */
export async function listTexts(userId: number, filter: TextFilter, page: number, itemPerPage: number): Promise<TextList> {
  const db = getDb();
  const where = filterConditions(userId, filter);
  const [rows, [{ total }]] = await Promise.all([
    db
      .select(listColumns)
      .from(texts)
      .where(where)
      .orderBy(desc(texts.lastReadAt))
      .limit(itemPerPage)
      .offset(itemPerPage * (page - 1)),
    db.select({ total: count() }).from(texts).where(where),
  ]);
  return { total, items: rows.map(toTextItem) };
}

export async function createText(session: Session, input: TextInput) {
  validate(input);
  return getDb().transaction(async (tx) => {
    const [{ id }] = await tx
      .insert(texts)
      .values({ ...input, userId: session.userId, termCount: 0, processedTermCount: 0 })
      .returning({ id: texts.id });
    await addLog(tx, 'textCreated', { Title: input.title, LanguageCode: input.languageCode }, session.userName, {
      type: 'Text',
      id,
    });
    return id;
  });
}

/** Editing resets the term counts so the background job splits the text again. */
export async function editText(id: number, userId: number, input: TextInput) {
  validate(input);
  await getOwnedText(id, userId);
  await getDb()
    .update(texts)
    .set({ ...input, termCount: 0, processedTermCount: 0 })
    .where(eq(texts.id, id));
}

export async function deleteText(id: number, userId: number) {
  await getOwnedText(id, userId);
  // text_terms rows go with it (ON DELETE CASCADE).
  await getDb().delete(texts).where(eq(texts.id, id));
}

export async function getTextEditDetail(id: number, userId: number): Promise<TextEditDetail> {
  const { title, languageCode, content } = await getOwnedText(id, userId);
  return { title, languageCode, content };
}

/** Opens a text for reading: bumps LastReadAt and records it as the last read text. */
export async function readText(id: number, session: Session): Promise<TextReadInfo> {
  const db = getDb();
  const [text] = await db
    .select({
      id: texts.id,
      title: texts.title,
      bookmark: texts.bookmark,
      languageCode: texts.languageCode,
      termCount: texts.termCount,
      processedTermCount: texts.processedTermCount,
    })
    .from(texts)
    .where(and(eq(texts.id, id), eq(texts.userId, session.userId)));
  if (!text) throw new UserError('Text not found.', 404);
  await db.transaction(async (tx) => {
    await tx.update(texts).set({ lastReadAt: new Date() }).where(eq(texts.id, id));
    await addLog(tx, 'textRead', { TextId: id, LanguageCode: text.languageCode }, session.userName, { type: 'Text', id });
  });
  return text;
}

export async function setBookmark(id: number, userId: number, termIndex: number) {
  const db = getDb();
  const [text] = await db
    .select({ termCount: texts.termCount })
    .from(texts)
    .where(and(eq(texts.id, id), eq(texts.userId, userId)));
  if (!text) throw new UserError("You don't have permission to access this text.", 403);
  if (!Number.isInteger(termIndex) || termIndex < 0 || termIndex >= text.termCount) {
    throw new UserError('Invalid bookmark index.');
  }
  await db.update(texts).set({ bookmark: termIndex }).where(eq(texts.id, id));
}

function emptyCounts(): Required<TermCounts> {
  return Object.fromEntries(Object.values(LearningLevel).map((l) => [l, 0])) as Required<TermCounts>;
}

/** Tokens per learning level; separators and punctuation count as skipped. */
export async function getTermCounts(id: number, userId: number) {
  const rows = await getDb()
    .select({ learningLevel: terms.learningLevel, n: count() })
    .from(textTerms)
    .innerJoin(texts, eq(textTerms.textId, texts.id))
    .leftJoin(terms, eq(textTerms.termId, terms.id))
    .where(and(eq(textTerms.textId, id), eq(texts.userId, userId)))
    .groupBy(terms.learningLevel);
  const result = emptyCounts();
  for (const row of rows) {
    const level = (row.learningLevel ?? LearningLevel.Skipped) as LearningLevel;
    result[level] += row.n;
  }
  return result;
}

const HIDDEN_MEANING = new Set<string>([LearningLevel.Ignored, LearningLevel.Skipped, LearningLevel.WellKnown]);

/** Tokens with index in [indexFrom, indexTo], each term with how often it occurs in the text. */
export async function getTextTerms(id: number, userId: number, indexFrom: number, indexTo: number): Promise<TextTermView[]> {
  const db = getDb();
  const rows = await db
    .select({
      content: textTerms.content,
      termId: textTerms.termId,
      index: textTerms.index,
      learningLevel: terms.learningLevel,
      meaning: terms.meaning,
      phrase: terms.phrase,
      autoJoin: terms.autoJoin,
    })
    .from(textTerms)
    .innerJoin(texts, eq(textTerms.textId, texts.id))
    .leftJoin(terms, eq(textTerms.termId, terms.id))
    .where(
      and(
        eq(texts.userId, userId),
        eq(textTerms.textId, id),
        gte(textTerms.index, indexFrom),
        lte(textTerms.index, indexTo),
      ),
    )
    .orderBy(asc(textTerms.index));

  const termIds = [...new Set(rows.flatMap((r) => (r.termId == null ? [] : [r.termId])))];
  const counts = new Map<number, number>();
  if (termIds.length) {
    const countRows = await db
      .select({ termId: textTerms.termId, n: count() })
      .from(textTerms)
      .where(and(eq(textTerms.textId, id), inArray(textTerms.termId, termIds)))
      .groupBy(textTerms.termId);
    for (const r of countRows) counts.set(r.termId!, r.n);
  }

  return rows.map((r) => {
    const learningLevel = (r.termId == null ? LearningLevel.Skipped : r.learningLevel) as LearningLevel;
    return {
      id: r.termId,
      content: r.content,
      learningLevel,
      index: r.index,
      meaning: HIDDEN_MEANING.has(learningLevel) ? null : (r.meaning ?? null),
      count: r.termId == null ? null : (counts.get(r.termId) ?? 0),
      phrase: r.phrase ?? false,
      autoJoin: r.autoJoin ?? false,
    };
  });
}

export async function getTermCountInText(id: number, userId: number, termId: number) {
  const [{ n }] = await getDb()
    .select({ n: count() })
    .from(textTerms)
    .innerJoin(texts, eq(textTerms.textId, texts.id))
    .where(and(eq(textTerms.textId, id), eq(texts.userId, userId), eq(textTerms.termId, termId)));
  return n;
}

export async function countTextsByLanguage(userId: number): Promise<Record<string, number>> {
  const rows = await getDb()
    .select({ languageCode: texts.languageCode, n: count() })
    .from(texts)
    .where(eq(texts.userId, userId))
    .groupBy(texts.languageCode);
  return Object.fromEntries(rows.map((r) => [r.languageCode, r.n]));
}

export async function getLastReadText(session: Session): Promise<TextItem | null> {
  const db = getDb();
  const [log] = await db
    .select({ entityId: logs.entityId })
    .from(logs)
    .where(and(eq(logs.userName, session.userName), eq(logs.event, 'textRead'), isNotNull(logs.entityId)))
    .orderBy(desc(logs.id))
    .limit(1);
  if (!log) return null;
  const [text] = await db
    .select(listColumns)
    .from(texts)
    .where(and(eq(texts.id, log.entityId!), eq(texts.userId, session.userId)));
  return text ? toTextItem(text) : null;
}

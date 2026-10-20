import 'server-only';
import { and, count, eq, notInArray } from 'drizzle-orm';
import { getDb, schema } from './db';
import { UserError } from './errors';
import { addLog } from './logs';
import { isLanguageCode } from '@/lib/languages';
import type { Session } from './auth/session';
import { isLearningLevel, LearningLevel, type TermView } from '@/lib/types';

const { terms } = schema;

async function getOwnedTerm(termId: number, userId: number) {
  const [term] = await getDb()
    .select()
    .from(terms)
    .where(and(eq(terms.id, termId), eq(terms.userId, userId)));
  if (!term) throw new UserError('term not found', 404);
  return term;
}

export async function getTerm(termId: number, userId: number): Promise<TermView> {
  const { id, content, meaning, learningLevel, languageCode } = await getOwnedTerm(termId, userId);
  return { id, content, meaning, learningLevel: learningLevel as LearningLevel, languageCode };
}

export async function getTermMeaning(termId: number, userId: number) {
  const { id, meaning } = await getOwnedTerm(termId, userId);
  return { id, meaning };
}

/** Terms are created as "unknown" when a text is split; this is for adding one by hand. */
export async function createTerm(
  session: Session,
  input: { content: string; meaning: string; learningLevel: string; languageCode: string },
) {
  if (!isLanguageCode(input.languageCode)) throw new UserError('Unsupported language.');
  if (!isLearningLevel(input.learningLevel)) throw new UserError('Invalid learning level.');
  const content = input.content.toUpperCase();
  const db = getDb();
  const [existing] = await db
    .select({ id: terms.id })
    .from(terms)
    .where(and(eq(terms.userId, session.userId), eq(terms.languageCode, input.languageCode), eq(terms.content, content)));
  if (existing) throw new UserError('Term has already exist.');
  return db.transaction(async (tx) => {
    const [{ id }] = await tx
      .insert(terms)
      .values({ ...input, content, userId: session.userId })
      .$returningId();
    await addLog(
      tx,
      'termCreated',
      { LanguageCode: input.languageCode, Content: content, LearningLevel: input.learningLevel },
      session.userName,
      { type: 'Term', id },
    );
    return id;
  });
}

export async function editTerm(session: Session, termId: number, input: { meaning: string; learningLevel: string }) {
  if (!isLearningLevel(input.learningLevel)) throw new UserError('Invalid learning level.');
  const current = await getOwnedTerm(termId, session.userId);
  const snapshot = (learningLevel: string) => ({
    Content: current.content,
    LanguageCode: current.languageCode,
    LearningLevel: learningLevel,
  });
  await getDb().transaction(async (tx) => {
    await tx.update(terms).set(input).where(eq(terms.id, termId));
    await addLog(
      tx,
      'termEdited',
      { Before: snapshot(current.learningLevel), After: snapshot(input.learningLevel) },
      session.userName,
      { type: 'Term', id: termId },
    );
  });
}

// Statistics only count terms the user is actually learning or knows.
const NOT_LEARNING = [LearningLevel.Ignored, LearningLevel.Skipped, LearningLevel.Unknown];

export async function countTermsByLanguage(userId: number): Promise<Record<string, number>> {
  const rows = await getDb()
    .select({ key: terms.languageCode, n: count() })
    .from(terms)
    .where(and(eq(terms.userId, userId), notInArray(terms.learningLevel, NOT_LEARNING)))
    .groupBy(terms.languageCode);
  return Object.fromEntries(rows.map((r) => [r.key, r.n]));
}

export async function countTermsByLearningLevel(userId: number): Promise<Record<string, number>> {
  const rows = await getDb()
    .select({ key: terms.learningLevel, n: count() })
    .from(terms)
    .where(and(eq(terms.userId, userId), notInArray(terms.learningLevel, NOT_LEARNING)))
    .groupBy(terms.learningLevel);
  return Object.fromEntries(rows.map((r) => [r.key, r.n]));
}

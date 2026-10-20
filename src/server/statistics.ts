import 'server-only';
import { and, count, eq, gte, inArray, notInArray, sql } from 'drizzle-orm';
import { getDb, schema } from './db';
import type { Session } from './auth/session';
import { utcOffset, type ActivityDay } from '@/lib/activity';
import { LearningLevel, type TermCounts } from '@/lib/types';

const { texts, terms, logs } = schema;

export type LanguageStatistics = { languageCode: string; texts: number; levels: TermCounts };

/** Texts and terms (by level, without ignored and skipped ones) of every language the user has either in. */
export async function getLanguageStatistics(userId: number): Promise<LanguageStatistics[]> {
  const db = getDb();
  const [textRows, termRows] = await Promise.all([
    db
      .select({ languageCode: texts.languageCode, n: count() })
      .from(texts)
      .where(eq(texts.userId, userId))
      .groupBy(texts.languageCode),
    db
      .select({ languageCode: terms.languageCode, level: terms.learningLevel, n: count() })
      .from(terms)
      .where(
        and(eq(terms.userId, userId), notInArray(terms.learningLevel, [LearningLevel.Ignored, LearningLevel.Skipped])),
      )
      .groupBy(terms.languageCode, terms.learningLevel),
  ]);
  const byCode = new Map<string, LanguageStatistics>();
  const entry = (languageCode: string) => {
    let stats = byCode.get(languageCode);
    if (!stats) byCode.set(languageCode, (stats = { languageCode, texts: 0, levels: {} }));
    return stats;
  };
  for (const r of textRows) entry(r.languageCode).texts = r.n;
  for (const r of termRows) entry(r.languageCode).levels[r.level as LearningLevel] = r.n;
  return [...byCode.values()];
}

const knownTimeZones = new Map<string, boolean>();

/**
 * Browsers report some zones by names newer tzdata only keeps in a legacy package Postgres may lack
 * (Chrome says Asia/Saigon), so unknown names fall back to the zone's current UTC offset.
 */
async function pgTimeZone(timeZone: string) {
  let known = knownTimeZones.get(timeZone);
  if (known === undefined) {
    const { rows } = await getDb().execute(sql`select 1 from pg_timezone_names where name = ${timeZone}`);
    knownTimeZones.set(timeZone, (known = rows.length > 0));
  }
  return known ? sql`${timeZone}` : sql`${utcOffset(timeZone)}::interval`;
}

/**
 * Terms marked and texts read per day over the last `days` days (and a bit), from the logs table,
 * split into days in `timeZone`. Only days with activity are returned.
 */
export async function getActivity(
  session: Session,
  days: number,
  timeZone: string,
  languageCode?: string,
): Promise<ActivityDay[]> {
  const conditions = [
    eq(logs.userName, session.userName),
    gte(logs.createdDate, new Date(Date.now() - (days + 1) * 86_400_000)),
    inArray(logs.event, ['termCreated', 'termEdited', 'textRead']),
  ];
  if (languageCode) {
    // termEdited keeps the language in its Before/After snapshots; the others at the top level.
    conditions.push(
      sql`coalesce(${logs.data}::jsonb ->> 'LanguageCode', ${logs.data}::jsonb -> 'Before' ->> 'LanguageCode') = ${languageCode}`,
    );
  }
  const zone = await pgTimeZone(timeZone);
  const rows = await getDb()
    .select({
      date: sql<string>`to_char(${logs.createdDate} at time zone ${zone}, 'YYYY-MM-DD')`,
      marked: sql<number>`count(*) filter (where ${logs.event} <> 'textRead')`.mapWith(Number),
      read: sql<number>`count(distinct ${logs.entityId}) filter (where ${logs.event} = 'textRead')`.mapWith(Number),
    })
    .from(logs)
    .where(and(...conditions))
    // By position: the time zone is a bound parameter, so repeating the expression wouldn't match.
    .groupBy(sql`1`);
  return rows;
}

import { existsSync, readFileSync } from 'node:fs';
import path from 'node:path';
import { gunzipSync } from 'node:zlib';
import { and, eq, sql } from 'drizzle-orm';
import { getDb, schema } from './db';

// Loads the dictionaries built by scripts/build-dictionary.mjs into Postgres. Runs in the background at startup and
// only re-imports a dictionary whose file changed, so a restart with the same image costs one small query.

const { dictionaries, dictionaryEntries } = schema;

type Manifest = Record<string, { file: string; source: string; words: number; sha256: string }>;

const BATCH_SIZE = 5000;
// Any constant; keeps two app instances starting together from importing the same rows.
const LOCK_KEY = 0x6c77_7464;

const unescape = (s: string) => s.replace(/\\(\\|n)/g, (_, c: string) => (c === 'n' ? '\n' : '\\'));

export async function importDictionaries(dir = path.join(process.cwd(), 'dictionary')) {
  const manifestPath = path.join(dir, 'manifest.json');
  if (!existsSync(manifestPath)) {
    console.warn('No built-in dictionary found; run `npm run build-dictionary` to enable dictionary suggestions.');
    return;
  }
  const manifest = JSON.parse(readFileSync(manifestPath, 'utf8')) as Manifest;
  const db = getDb();
  for (const [pair, info] of Object.entries(manifest)) {
    const [languageCode, dictionaryLanguageCode] = pair.split('-');
    const samePair = and(
      eq(dictionaries.languageCode, languageCode),
      eq(dictionaries.dictionaryLanguageCode, dictionaryLanguageCode),
    );
    const [current] = await db.select({ sha256: dictionaries.sha256 }).from(dictionaries).where(samePair);
    if (current?.sha256 === info.sha256) continue;

    const started = Date.now();
    const rows = gunzipSync(readFileSync(path.join(dir, info.file)))
      .toString('utf8')
      .split('\n')
      .filter(Boolean)
      .map((line) => {
        const tab = line.indexOf('\t');
        return [unescape(line.slice(0, tab)), unescape(line.slice(tab + 1))];
      });
    // One transaction, so lookups keep seeing the old dictionary until the new one is complete.
    const imported = await db.transaction(async (tx) => {
      await tx.execute(sql`SELECT pg_advisory_xact_lock(${LOCK_KEY})`);
      const [again] = await tx.select({ sha256: dictionaries.sha256 }).from(dictionaries).where(samePair);
      if (again?.sha256 === info.sha256) return false;
      await tx
        .delete(dictionaryEntries)
        .where(
          and(
            eq(dictionaryEntries.languageCode, languageCode),
            eq(dictionaryEntries.dictionaryLanguageCode, dictionaryLanguageCode),
          ),
        );
      for (let i = 0; i < rows.length; i += BATCH_SIZE) {
        const batch = rows.slice(i, i + BATCH_SIZE);
        await tx.execute(sql`
          INSERT INTO dictionary_entries (language_code, dictionary_language_code, word, meaning)
          SELECT ${languageCode}, ${dictionaryLanguageCode}, w, m
          FROM unnest(${sql.param(batch.map((r) => r[0]))}::text[], ${sql.param(batch.map((r) => r[1]))}::text[]) AS t(w, m)`);
      }
      const values = { source: info.source, sha256: info.sha256, importedAt: new Date() };
      await tx
        .insert(dictionaries)
        .values({ languageCode, dictionaryLanguageCode, ...values })
        .onConflictDoUpdate({ target: [dictionaries.languageCode, dictionaries.dictionaryLanguageCode], set: values });
      return true;
    });
    if (imported) console.log(`Imported the ${pair} dictionary (${rows.length} words) in ${Date.now() - started} ms`);
  }
}

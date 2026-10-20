import 'server-only';
import nlp from 'compromise';
import { and, eq, inArray } from 'drizzle-orm';
import { getDb, schema } from './db';
import { japaneseBaseForm } from './languages/japanese';

// Lookups in the built-in dictionaries (see src/server/dictionaryImport.ts). A word is tried as written first, then
// in its dictionary form: "walked" -> "walk" for English, 食べ -> 食べる for Japanese.

const { dictionaryEntries } = schema;

// Inflections WordNet lists under the base form; tried after compromise's guess.
const ENGLISH_SUFFIXES: [string, string][] = [
  ['ies', 'y'],
  ['es', ''],
  ['s', ''],
  ['ied', 'y'],
  ['ed', 'e'],
  ['ed', ''],
  ['ing', 'e'],
  ['ing', ''],
  ['er', ''],
  ['est', ''],
];

function englishCandidates(word: string) {
  const lower = word.toLowerCase();
  const doc = nlp(lower);
  doc.verbs().toInfinitive();
  doc.nouns().toSingular();
  const candidates = [lower, doc.text().toLowerCase()];
  for (const [suffix, replacement] of ENGLISH_SUFFIXES) {
    if (lower.length > suffix.length + 1 && lower.endsWith(suffix)) candidates.push(lower.slice(0, -suffix.length) + replacement);
  }
  return candidates;
}

async function candidates(word: string, languageCode: string): Promise<string[]> {
  const trimmed = word.trim();
  if (languageCode === 'en') return englishCandidates(trimmed);
  if (languageCode === 'ja') return [trimmed, (await japaneseBaseForm(trimmed)) ?? trimmed];
  return [trimmed];
}

export async function lookUpMeaning(word: string, from: string, to: string): Promise<string | null> {
  const words = [...new Set((await candidates(word, from)).filter(Boolean))];
  if (!words.length) return null;
  const rows = await getDb()
    .select({ word: dictionaryEntries.word, meaning: dictionaryEntries.meaning })
    .from(dictionaryEntries)
    .where(
      and(
        eq(dictionaryEntries.languageCode, from),
        eq(dictionaryEntries.dictionaryLanguageCode, to),
        inArray(dictionaryEntries.word, words),
      ),
    );
  for (const w of words) {
    const row = rows.find((r) => r.word === w);
    if (row) return row.meaning;
  }
  return null;
}

import 'server-only';
import nlp from 'compromise';
import { and, eq, inArray } from 'drizzle-orm';
import { getDb, schema } from './db';
import { japaneseBaseForm } from './languages/japanese';

// Lookups in the built-in dictionaries (see src/server/dictionaryImport.ts), then in Microsoft Translator's dictionary
// when the user saved an Azure Translator key on their profile. A word is tried as written first, then in its
// dictionary form: "walked" -> "walk" for English, 食べ -> 食べる for Japanese.

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
  // A doubled final consonant before the suffix: "planned" -> "plan", "bigger" -> "big".
  const doubled = /^(.*([b-df-hj-np-tv-z]))\2(?:ed|ing|er|est)$/.exec(lower);
  if (doubled) candidates.push(doubled[1]);
  return candidates;
}

async function candidates(word: string, languageCode: string): Promise<string[]> {
  const trimmed = word.trim();
  if (languageCode === 'en') return englishCandidates(trimmed);
  if (languageCode === 'ja') return [trimmed, (await japaneseBaseForm(trimmed)) ?? trimmed];
  return [trimmed];
}

async function lookUpBuiltIn(words: string[], from: string, to: string) {
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

/** An Azure Translator resource's key, and its region unless it is a global resource. */
export type TranslatorSettings = { key: string; region: string | null };

// Microsoft's codes where they differ from ours.
const MICROSOFT_CODES: Record<string, string> = { zh: 'zh-Hans' };
// Words one dictionary lookup request may hold.
const MICROSOFT_MAX_WORDS = 10;
const TIMEOUT_MS = 5000;

type MicrosoftEntry = { translations: { displayTarget: string; posTag: string }[] };

/** Translations grouped by part of speech, most confident first: "noun: đồng hồ tốc độ, công tơ mét". */
function formatMicrosoftEntry(entry: MicrosoftEntry) {
  const byPos = new Map<string, string[]>();
  for (const t of entry.translations) {
    const pos = t.posTag.toLowerCase();
    byPos.set(pos, [...(byPos.get(pos) ?? []), t.displayTarget]);
  }
  return [...byPos].map(([pos, words]) => (pos === 'other' ? words.join(', ') : `${pos}: ${words.join(', ')}`)).join('\n') || null;
}

/** Microsoft Translator's dictionary; only pairs English with another language. Undefined when it could not be asked. */
async function lookUpMicrosoft(words: string[], from: string, to: string, { key, region }: TranslatorSettings) {
  const url = new URL('https://api.cognitive.microsofttranslator.com/dictionary/lookup');
  url.searchParams.set('api-version', '3.0');
  url.searchParams.set('from', MICROSOFT_CODES[from] ?? from);
  url.searchParams.set('to', MICROSOFT_CODES[to] ?? to);
  try {
    const res = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Ocp-Apim-Subscription-Key': key,
        ...(region ? { 'Ocp-Apim-Subscription-Region': region } : {}),
      },
      body: JSON.stringify(words.slice(0, MICROSOFT_MAX_WORDS).map((Text) => ({ Text }))),
      signal: AbortSignal.timeout(TIMEOUT_MS),
    });
    // 400 is a language pair it has no dictionary for, which asking again won't change.
    if (res.status === 400) return null;
    if (!res.ok) return undefined;
    const entries = (await res.json()) as MicrosoftEntry[];
    const entry = entries.find((e) => e.translations.length > 0);
    return entry ? formatMicrosoftEntry(entry) : null;
  } catch {
    return undefined;
  }
}

/** The meaning; null when no dictionary has the word, undefined when Microsoft could not be asked. */
export async function lookUpMeaning(
  word: string,
  from: string,
  to: string,
  translator: TranslatorSettings | null,
): Promise<string | null | undefined> {
  const words = [...new Set((await candidates(word, from)).filter(Boolean))];
  if (!words.length) return null;
  const meaning = await lookUpBuiltIn(words, from, to);
  if (meaning || !translator) return meaning;
  return lookUpMicrosoft(words, from, to, translator);
}

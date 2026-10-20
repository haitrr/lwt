import 'server-only';
import nlp from 'compromise';

// Dictionaries asked in turn until one has the word:
// - The service the old app used (lwt-dictionary), when LWT_DICTIONARY_API_ROOT is set:
//   GET {root}/dictionary/{word}?fromLang=&toLang= answering { meaning }, or 404 when it has no entry.
// - Microsoft Translator's dictionary lookup, when the user saved an Azure Translator key on their profile.
//   It only pairs English with another language.

/** The meaning, null when the dictionary has no entry, undefined when it could not be asked. */
type Lookup = (word: string, from: string, to: string) => Promise<string | null | undefined>;

const TIMEOUT_MS = 5000;

/** Dictionary form of a word: "walked" -> "walk", "books" -> "book" for English. */
function dictionaryForm(word: string, languageCode: string) {
  if (languageCode !== 'en') return word.toLowerCase();
  const doc = nlp(word);
  doc.verbs().toInfinitive();
  doc.nouns().toSingular();
  return doc.text().toLowerCase() || word.toLowerCase();
}

function lwtDictionary(): Lookup | null {
  const root = process.env.LWT_DICTIONARY_API_ROOT;
  if (!root) return null;
  return async (word, from, to) => {
    const url = new URL(`dictionary/${encodeURIComponent(word)}`, root.endsWith('/') ? root : `${root}/`);
    url.searchParams.set('fromLang', from);
    url.searchParams.set('toLang', to);
    try {
      const res = await fetch(url, { signal: AbortSignal.timeout(TIMEOUT_MS) });
      if (res.status === 404) return null;
      if (!res.ok) return undefined;
      const body = (await res.json()) as { meaning?: string };
      return body.meaning || null;
    } catch {
      return undefined;
    }
  };
}

// Microsoft's codes where they differ from ours.
const MICROSOFT_CODES: Record<string, string> = { zh: 'zh-Hans' };

type MicrosoftEntry = { translations: { displayTarget: string; posTag: string }[] };

/** Translations grouped by part of speech, most confident first: "noun: đồng hồ tốc độ, công tơ mét". */
function formatMicrosoftEntry(entry: MicrosoftEntry | undefined) {
  const byPos = new Map<string, string[]>();
  for (const t of entry?.translations ?? []) {
    const pos = t.posTag.toLowerCase();
    byPos.set(pos, [...(byPos.get(pos) ?? []), t.displayTarget]);
  }
  return [...byPos].map(([pos, words]) => (pos === 'other' ? words.join(', ') : `${pos}: ${words.join(', ')}`)).join('\n') || null;
}

/** An Azure Translator resource's key, and its region unless it is a global resource. */
export type TranslatorSettings = { key: string; region: string | null };

function microsoftDictionary(translator: TranslatorSettings | null): Lookup | null {
  if (!translator) return null;
  const { key, region } = translator;
  return async (word, from, to) => {
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
        body: JSON.stringify([{ Text: word }]),
        signal: AbortSignal.timeout(TIMEOUT_MS),
      });
      // 400 is a language pair it has no dictionary for, which asking again won't change.
      if (res.status === 400) return null;
      if (!res.ok) return undefined;
      const [entry] = (await res.json()) as MicrosoftEntry[];
      return formatMicrosoftEntry(entry);
    } catch {
      return undefined;
    }
  };
}

/** The first meaning any configured dictionary has; null when none has the word, undefined when one could not be asked. */
export async function lookUpMeaning(
  word: string,
  from: string,
  to: string,
  translator: TranslatorSettings | null,
): Promise<string | null | undefined> {
  const dictionaries = [lwtDictionary(), microsoftDictionary(translator)].filter((d) => d != null);
  if (dictionaries.length === 0) return undefined;
  const form = dictionaryForm(word, from);
  let failed = false;
  for (const lookUp of dictionaries) {
    const meaning = await lookUp(form, from, to);
    if (meaning) return meaning;
    if (meaning === undefined) failed = true;
  }
  return failed ? undefined : null;
}

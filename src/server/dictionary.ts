import 'server-only';
import nlp from 'compromise';

// External dictionary service the old app used (lwt-dictionary): GET {root}/dictionary/{word}?fromLang=&toLang=
// answering { meaning }. Optional; without LWT_DICTIONARY_API_ROOT lookups just return nothing.

/** Dictionary form of a word: "walked" -> "walk", "books" -> "book" for English. */
function dictionaryForm(word: string, languageCode: string) {
  if (languageCode !== 'en') return word.toLowerCase();
  const doc = nlp(word);
  doc.verbs().toInfinitive();
  doc.nouns().toSingular();
  return doc.text().toLowerCase() || word.toLowerCase();
}

export async function lookUpMeaning(word: string, from: string, to: string): Promise<string | null> {
  const root = process.env.LWT_DICTIONARY_API_ROOT;
  if (!root) return null;
  const url = new URL(`dictionary/${encodeURIComponent(dictionaryForm(word, from))}`, root.endsWith('/') ? root : `${root}/`);
  url.searchParams.set('fromLang', from);
  url.searchParams.set('toLang', to);
  try {
    const res = await fetch(url, { signal: AbortSignal.timeout(5000) });
    if (!res.ok) return null;
    const body = (await res.json()) as { meaning?: string };
    return body.meaning || null;
  } catch {
    return null;
  }
}

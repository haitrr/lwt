// Phrases are terms the user made from several words, like "John Maynard Keynes", that the splitter cut apart.
// Wherever their words appear in a row, they are stored as one token and one term.

/** A token in reading order: its text as written, and its term key (normalized content) unless it's a separator. */
export type PhraseToken = { text: string; key: string | null };

/** Separators that may sit between a phrase's words: spaces with at most one line break, not a paragraph break. */
function isGap(text: string) {
  return /^\s+$/.test(text);
}

/**
 * Walks words from tokens[start], calling onWord with the phrase key built so far each time another word joins.
 * Words are joined by separator; with an empty separator (Chinese, Japanese) only directly adjacent words join.
 * Stops at punctuation, at a paragraph break, or when onWord returns false.
 */
function walk(tokens: PhraseToken[], start: number, separator: string, onWord: (key: string, end: number) => boolean) {
  const first = tokens[start]?.key;
  if (first == null || !onWord(first, start)) return;
  let key = first;
  let newlines = 0;
  for (let i = start + 1; i < tokens.length; i++) {
    const token = tokens[i];
    if (token.key == null) {
      if (!separator || !isGap(token.text)) return;
      newlines += token.text.split('\n').length - 1;
      if (newlines > 1) return;
      continue;
    }
    key += separator + token.key;
    newlines = 0;
    if (!onWord(key, i)) return;
  }
}

/** The key of a phrase made of exactly these tokens, or null when they can't form one (one word, punctuation inside). */
export function phraseKey(tokens: PhraseToken[], separator: string): string | null {
  let result: string | null = null;
  walk(tokens, 0, separator, (key, end) => {
    if (end === tokens.length - 1 && end > 0) result = key;
    return true;
  });
  return result;
}

export type PhraseSet = { phrases: Set<string>; prefixes: Set<string>; separator: string };

export function buildPhraseSet(keys: Iterable<string>, separator: string): PhraseSet {
  const phrases = new Set(keys);
  const prefixes = new Set<string>();
  for (const phrase of phrases) {
    for (let i = 1; i <= phrase.length; i++) prefixes.add(phrase.slice(0, i));
  }
  return { phrases, prefixes, separator };
}

/** Index of the last token of the longest phrase starting at tokens[start], or null when none does. */
export function matchPhrase(tokens: PhraseToken[], start: number, set: PhraseSet): number | null {
  if (set.phrases.size === 0) return null;
  let best: number | null = null;
  walk(tokens, start, set.separator, (key, end) => {
    if (!set.prefixes.has(key)) return false;
    if (end > start && set.phrases.has(key)) best = end;
    return true;
  });
  return best;
}

/** One token after merging phrases: the index of its first original token, its text, and its term key. */
export type MergedToken = { index: number; text: string; key: string | null };

/**
 * Merges tokens[from..] into phrases until at least `count` original tokens are consumed.
 * A merged token keeps the index of its first word, so the indices of the words it swallowed are left unused.
 */
export function mergePhrases(tokens: PhraseToken[], from: number, count: number, set: PhraseSet) {
  const merged: MergedToken[] = [];
  let i = from;
  while (i < tokens.length && i < from + count) {
    const end = matchPhrase(tokens, i, set);
    if (end == null) {
      merged.push({ index: i, text: tokens[i].text, key: tokens[i].key });
      i++;
    } else {
      const text = tokens
        .slice(i, end + 1)
        .map((t) => t.text)
        .join('');
      merged.push({ index: i, text, key: walkKey(tokens, i, end, set.separator) });
      i = end + 1;
    }
  }
  return { merged, next: i };
}

function walkKey(tokens: PhraseToken[], start: number, end: number, separator: string) {
  return phraseKey(tokens.slice(start, end + 1), separator)!;
}

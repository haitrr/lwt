// Phrases are terms the user made from tokens the splitter cut apart, like "John Maynard Keynes", "GPT-6" or "U.S.".
// Wherever their tokens appear in a row, they are stored as one token and one term.

/** A token in reading order: its text as written, and its term key (normalized content) unless it's a separator. */
export type PhraseToken = { text: string; key: string | null };

/** Separators that may sit between a phrase's words: spaces with at most one line break, not a paragraph break. */
function isGap(text: string) {
  return /^\s+$/.test(text);
}

/**
 * Walks tokens from tokens[start], which must be a word, calling onToken with the phrase key built so far each time
 * another word, number or punctuation mark joins. Tokens written together join as they are ("GPT-6"); spaces between
 * them read as one separator, and with an empty separator (Chinese, Japanese) spaces aren't allowed.
 * Stops at a paragraph break, or when onToken returns false.
 */
function walk(tokens: PhraseToken[], start: number, separator: string, onToken: (key: string, end: number) => boolean) {
  const first = tokens[start]?.key;
  if (first == null || !onToken(first, start)) return;
  let key = first;
  let gap = false;
  let newlines = 0;
  for (let i = start + 1; i < tokens.length; i++) {
    const token = tokens[i];
    if (token.key == null && isGap(token.text)) {
      if (!separator) return;
      newlines += token.text.split('\n').length - 1;
      if (newlines > 1) return;
      gap = true;
      continue;
    }
    // Words of a separated language were always joined by the separator; only punctuation and numbers sit close.
    const close = !gap && (token.key == null || tokens[i - 1].key == null || !separator);
    key += (close ? '' : separator) + (token.key ?? token.text.toUpperCase());
    gap = false;
    newlines = 0;
    if (!onToken(key, i)) return;
  }
}

/** The key of a phrase made of exactly these tokens, or null when they can't form one (one token, not starting with a word, a paragraph break). */
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

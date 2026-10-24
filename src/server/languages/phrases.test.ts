import { describe, expect, it } from 'vitest';
import { getLanguage, termKey } from '.';
import { buildPhraseSet, mergePhrases, phraseKey } from './phrases';

async function tokensOf(code: string, text: string) {
  const language = getLanguage(code);
  const words = await language.split(text);
  return words.map((w) => ({ text: w, key: termKey(language, w) }));
}

async function merge(code: string, text: string, phrases: string[]) {
  const tokens = await tokensOf(code, text);
  const set = buildPhraseSet(phrases, getLanguage(code).phraseSeparator);
  return mergePhrases(tokens, 0, tokens.length, set).merged;
}

describe('phrases', () => {
  it('merges a name into one token that keeps the index of its first word', async () => {
    const merged = await merge('en', 'See John Maynard Keynes, today.', ['JOHN MAYNARD KEYNES']);
    expect(merged.map((t) => t.text)).toEqual(['See', ' ', 'John Maynard Keynes', ',', ' ', 'today', '.']);
    expect(merged[2]).toEqual({ index: 2, text: 'John Maynard Keynes', key: 'JOHN MAYNARD KEYNES' });
    expect(merged[3].index).toBe(7);
  });

  it('prefers the longest phrase and ignores case', async () => {
    const merged = await merge('en', 'john maynard keynes and Maynard Keynes', ['MAYNARD KEYNES', 'JOHN MAYNARD KEYNES']);
    expect(merged.map((t) => t.key)).toEqual(['JOHN MAYNARD KEYNES', null, 'AND', null, 'MAYNARD KEYNES']);
  });

  it('joins across a line break but not punctuation or a paragraph', async () => {
    const phrases = ['QUANTITATIVE EASING'];
    expect((await merge('en', 'quantitative\neasing', phrases)).length).toBe(1);
    expect((await merge('en', 'quantitative  easing', phrases)).length).toBe(1);
    expect((await merge('en', 'quantitative, easing', phrases)).length).toBe(4);
    expect((await merge('en', 'quantitative\n\neasing', phrases)).length).toBe(4);
  });

  it('does not merge a prefix of a phrase', async () => {
    const merged = await merge('en', 'John Maynard was here', ['JOHN MAYNARD KEYNES']);
    expect(merged.map((t) => t.text)).toEqual(['John', ' ', 'Maynard', ' ', 'was', ' ', 'here']);
  });

  it('joins adjacent Chinese words without a separator', async () => {
    const tokens = [
      { text: '凯', key: '凯' },
      { text: '恩斯', key: '恩斯' },
      { text: '说', key: '说' },
    ];
    const set = buildPhraseSet(['凯恩斯'], '');
    expect(mergePhrases(tokens, 0, tokens.length, set).merged.map((t) => t.text)).toEqual(['凯恩斯', '说']);
  });

  it('builds a phrase key only from two or more words in a row', async () => {
    expect(phraseKey(await tokensOf('en', 'John Maynard'), ' ')).toBe('JOHN MAYNARD');
    expect(phraseKey(await tokensOf('en', 'John'), ' ')).toBeNull();
    expect(phraseKey(await tokensOf('en', 'John, Maynard'), ' ')).toBeNull();
    expect(phraseKey(await tokensOf('en', 'John '), ' ')).toBeNull();
  });

  it('stops a chunk after a phrase that runs past it', async () => {
    const tokens = await tokensOf('en', 'a John Maynard Keynes b');
    const set = buildPhraseSet(['JOHN MAYNARD KEYNES'], ' ');
    const first = mergePhrases(tokens, 0, 3, set);
    expect(first.merged.map((t) => t.text)).toEqual(['a', ' ', 'John Maynard Keynes']);
    expect(first.next).toBe(7);
    expect(mergePhrases(tokens, first.next, 100, set).merged.map((t) => t.text)).toEqual([' ', 'b']);
  });
});

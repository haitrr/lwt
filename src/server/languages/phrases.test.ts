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

  it('joins across a line break but not a paragraph, and keeps punctuation in the key', async () => {
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

  it('builds a phrase key from two or more tokens that start with a word', async () => {
    expect(phraseKey(await tokensOf('en', 'John Maynard'), ' ')).toBe('JOHN MAYNARD');
    expect(phraseKey(await tokensOf('en', 'John'), ' ')).toBeNull();
    expect(phraseKey(await tokensOf('en', 'John '), ' ')).toBeNull();
    expect(phraseKey(await tokensOf('en', '(John'), ' ')).toBeNull();
    expect(phraseKey(await tokensOf('en', 'John\n\nMaynard'), ' ')).toBeNull();
  });

  it('keeps numbers and punctuation in a phrase as written, spaces as one separator', async () => {
    expect(phraseKey(await tokensOf('en', 'GPT-6'), ' ')).toBe('GPT-6');
    expect(phraseKey(await tokensOf('en', 'COVID-19'), ' ')).toBe('COVID-19');
    expect(phraseKey(await tokensOf('en', 'U.S.'), ' ')).toBe('U.S.');
    expect(phraseKey(await tokensOf('en', 'Mr.  Smith'), ' ')).toBe('MR. SMITH');
    expect(phraseKey(await tokensOf('en', 'John, Maynard'), ' ')).toBe('JOHN, MAYNARD');
    expect(phraseKey(await tokensOf('en', 'gpt- 6'), ' ')).toBe('GPT- 6');
  });

  it('merges a phrase with numbers and punctuation only where it is written the same', async () => {
    const merged = await merge('en', 'GPT-6 beats GPT-5 and gpt-6.', ['GPT-6']);
    expect(merged.filter((t) => t.key === 'GPT-6').map((t) => t.text)).toEqual(['GPT-6', 'gpt-6']);
    expect(merged.map((t) => t.text)).toContain('GPT-');
  });

  it('keeps punctuation in Chinese and Japanese phrases but no spaces', () => {
    const tokens = [
      { text: '東京', key: '東京' },
      { text: '・', key: null },
      { text: '大阪', key: '大阪' },
      { text: ' ', key: null },
      { text: '線', key: '線' },
    ];
    expect(phraseKey(tokens.slice(0, 3), '')).toBe('東京・大阪');
    expect(phraseKey(tokens, '')).toBeNull();
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

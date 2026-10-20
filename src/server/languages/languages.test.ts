import { describe, expect, it } from 'vitest';
import chinese from './__fixtures__/chinese.json';
import english from './__fixtures__/english.json';
import japanese from './__fixtures__/japanese.json';
import { LANGUAGES } from '@/lib/languages';
import { getLanguage, hasSplitter, MAX_TERM_LENGTH, termKey } from '.';

// Fixtures were recorded from the old .NET API, so existing texts keep splitting the same way.

// The .NET splitter cut emoji into two lone surrogates (stored as U+FFFD each); emoji now stay whole.
function oldEmoji(tokens: string[]) {
  return tokens.flatMap((t) => (/^\p{Extended_Pictographic}$/u.test(t) ? ['�', '�'] : [t]));
}

describe.each([
  ['en', english.cases],
  ['zh', chinese.cases],
  ['ja', japanese.cases],
])('%s splitting', (code, cases) => {
  it.each(cases.map((c, i) => [i, c] as const))('matches the old API on case %i', async (_, { text, tokens }) => {
    const result = await getLanguage(code).split(text);
    expect(oldEmoji(result)).toEqual(tokens);
  });
});

describe('term rules', () => {
  it('normalizes English to upper case and skips non-words', () => {
    const en = getLanguage('en');
    expect(en.normalize("it's")).toBe("IT'S");
    expect(en.shouldSkip('well-known')).toBe(false);
    expect(en.shouldSkip(' ')).toBe(true);
    expect(en.shouldSkip('3')).toBe(true);
  });

  it('skips English tokens without a letter', () => {
    const en = getLanguage('en');
    expect(en.split('a --- b')).toContain('---');
    expect(en.shouldSkip('---')).toBe(true);
    expect(en.shouldSkip("'")).toBe(true);
    expect(en.shouldSkip('’-')).toBe(true);
    expect(en.shouldSkip("o'clock")).toBe(false);
  });

  it('skips punctuation and Latin text in Chinese', () => {
    const zh = getLanguage('zh');
    expect(zh.shouldSkip('清华大学')).toBe(false);
    expect(zh.shouldSkip('。')).toBe(true);
    expect(zh.shouldSkip('abc')).toBe(true);
  });
});

describe('termKey', () => {
  const en = getLanguage('en');

  it('normalizes words and skips non-words', () => {
    expect(termKey(en, 'Hello')).toBe('HELLO');
    expect(termKey(en, ' ')).toBeNull();
    expect(termKey(en, '-'.repeat(200))).toBeNull();
  });

  it('skips words longer than terms.content can hold', () => {
    const slug = Array(30).fill('word').join('-');
    expect(en.split(`see ${slug} here`)).toContain(slug);
    expect(slug.length).toBeGreaterThan(MAX_TERM_LENGTH);
    expect(termKey(en, slug)).toBeNull();
    expect(termKey(en, 'a'.repeat(MAX_TERM_LENGTH))).toBe('A'.repeat(MAX_TERM_LENGTH));
  });

  it('counts characters, not UTF-16 units', () => {
    // 95 characters outside the BMP are 190 UTF-16 units but still fit varchar(95).
    const zh = { ...getLanguage('zh'), shouldSkip: () => false };
    expect(termKey(zh, '𠀀'.repeat(MAX_TERM_LENGTH))).not.toBeNull();
    expect(termKey(zh, '𠀀'.repeat(MAX_TERM_LENGTH + 1))).toBeNull();
  });
});

describe('text languages', () => {
  it('offers texts only in languages that have a splitter', () => {
    for (const l of LANGUAGES) expect(hasSplitter(l.code), l.code).toBe(l.supportsTexts);
    expect(hasSplitter('vi')).toBe(false);
  });
});

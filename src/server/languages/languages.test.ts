import { describe, expect, it } from 'vitest';
import chinese from './__fixtures__/chinese.json';
import english from './__fixtures__/english.json';
import japanese from './__fixtures__/japanese.json';
import { getLanguage } from '.';

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

  it('skips punctuation and Latin text in Chinese', () => {
    const zh = getLanguage('zh');
    expect(zh.shouldSkip('清华大学')).toBe(false);
    expect(zh.shouldSkip('。')).toBe(true);
    expect(zh.shouldSkip('abc')).toBe(true);
  });
});

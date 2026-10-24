import { describe, expect, it, vi } from 'vitest';
import { getLanguage } from './languages';
import { buildPhraseSet } from './languages/phrases';

vi.mock('server-only', () => ({}));
const { splitSegments } = await import('./webReading');

const none = (separator: string) => buildPhraseSet([], separator);

describe('splitSegments', () => {
  it('gives each word its offsets and key, leaving out separators', async () => {
    const [tokens] = await splitSegments(getLanguage('en'), ['Hello, big world!'], none(' '));
    expect(tokens).toEqual([
      { start: 0, end: 5, key: 'HELLO' },
      { start: 7, end: 10, key: 'BIG' },
      { start: 11, end: 16, key: 'WORLD' },
    ]);
  });

  it('keeps one result per segment', async () => {
    const result = await splitSegments(getLanguage('en'), ['one', '...', 'two'], none(' '));
    expect(result.map((s) => s.map((t) => t.key))).toEqual([['ONE'], [], ['TWO']]);
  });

  it('merges auto-join phrases over the whole span', async () => {
    const text = 'Ask John  Maynard Keynes.';
    const [tokens] = await splitSegments(getLanguage('en'), [text], buildPhraseSet(['JOHN MAYNARD KEYNES'], ' '));
    expect(tokens.map((t) => [text.slice(t.start, t.end), t.key])).toEqual([
      ['Ask', 'ASK'],
      ['John  Maynard Keynes', 'JOHN MAYNARD KEYNES'],
    ]);
  });

  it('finds Japanese words although the splitter drops spaces', async () => {
    const text = '猫が 好き';
    const [tokens] = await splitSegments(getLanguage('ja'), [text], none(''));
    for (const t of tokens) expect(text.slice(t.start, t.end)).toBe(t.key);
    expect(tokens.map((t) => t.key).join('')).toBe('猫が好き');
  });
});

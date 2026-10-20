import { describe, expect, it } from 'vitest';
import { buildJapanese, toneMarks } from './build-dictionary.mjs';

describe('toneMarks', () => {
  it('puts the mark on the right vowel', () => {
    expect(toneMarks('zhong1 guo2')).toBe('zhōng guó');
    expect(toneMarks('lu:4 you3 xiu1 gui4')).toBe('lǜ yǒu xiū guì');
    expect(toneMarks('Bei3 jing1 de5')).toBe('Běi jīng de');
    expect(toneMarks('A1 er3')).toBe('Ā ěr');
  });
});

describe('buildJapanese', () => {
  const sense = (gloss: string[], extra = {}) => ({
    appliesToKanji: ['*'],
    appliesToKana: ['*'],
    misc: [],
    gloss: gloss.map((text) => ({ text })),
    ...extra,
  });
  const jmdict = {
    words: [
      {
        kanji: [{ text: '紙', common: true }],
        kana: [{ text: 'かみ', common: true, appliesToKanji: ['*'] }],
        sense: [sense(['paper'])],
      },
      {
        kanji: [{ text: '是非', common: true }],
        kana: [{ text: 'ぜひ', common: true, appliesToKanji: ['*'] }],
        sense: [sense(['certainly', 'without fail'], { misc: ['uk'] }), sense(['right and wrong'])],
      },
    ],
  };

  it('keys entries by kanji and kana forms', () => {
    const dict = buildJapanese(jmdict);
    expect(dict.get('紙')).toEqual(['かみ: paper']);
    expect(dict.get('かみ')).toEqual(['紙: paper']);
    expect(dict.get('是非')).toEqual(['ぜひ: certainly, without fail; right and wrong']);
    expect(dict.get('ぜひ')).toEqual(['certainly, without fail; right and wrong']);
  });
});

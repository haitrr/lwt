import { cutChinese } from './jieba';
import { cutJapanese } from './japanese';

export type LanguageCode = 'en' | 'zh' | 'ja' | 'vi';

export type Language = {
  code: LanguageCode;
  /** Splits a text into the terms stored in text_terms, separators included. */
  split(text: string): string[] | Promise<string[]>;
  /** Terms that are punctuation, spaces, numbers, etc. and never become a term row. */
  shouldSkip(word: string): boolean;
  /** Key used for the terms table. */
  normalize(word: string): string;
};

const ENGLISH_SEPARATOR = /([^a-zA-Z’'-])/;

const english: Language = {
  code: 'en',
  split: (text) => text.split(ENGLISH_SEPARATOR).filter(Boolean),
  shouldSkip: (word) => /[^a-zA-Z’'-]/.test(word),
  normalize: (word) => word.toUpperCase(),
};

const chinese: Language = {
  code: 'zh',
  split: cutChinese,
  shouldSkip: (word) => !/^\p{Lo}+$/u.test(word),
  normalize: (word) => word,
};

const japanese: Language = {
  code: 'ja',
  split: cutJapanese,
  shouldSkip: (word) => !/^[぀-ゟ゠-ヿ＀-ﾟ一-龯㐀-䶿]+$/.test(word),
  normalize: (word) => word,
};

const unsupported = (): never => {
  throw new Error('Vietnamese is not supported yet.');
};

const vietnamese: Language = {
  code: 'vi',
  split: unsupported,
  shouldSkip: unsupported,
  normalize: unsupported,
};

const SPLITTERS: Language[] = [english, chinese, japanese, vietnamese];

export function getLanguage(code: string): Language {
  const language = SPLITTERS.find((l) => l.code === code);
  if (!language) throw new Error(`Language ${code} is not supported.`);
  return language;
}

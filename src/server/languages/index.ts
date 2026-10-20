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
  /** Joins the words of a phrase term: "JOHN MAYNARD KEYNES" in English, nothing in Chinese and Japanese. */
  phraseSeparator: string;
};

const ENGLISH_SEPARATOR = /([^a-zA-Z’'-])/;

const english: Language = {
  code: 'en',
  split: (text) => text.split(ENGLISH_SEPARATOR).filter(Boolean),
  // Runs of only dashes or apostrophes (a "----" rule, a lone quote) are not words.
  shouldSkip: (word) => /[^a-zA-Z’'-]/.test(word) || !/[a-zA-Z]/.test(word),
  normalize: (word) => word.toUpperCase(),
  phraseSeparator: ' ',
};

const chinese: Language = {
  code: 'zh',
  split: cutChinese,
  shouldSkip: (word) => !/^\p{Lo}+$/u.test(word),
  normalize: (word) => word,
  phraseSeparator: '',
};

const japanese: Language = {
  code: 'ja',
  split: cutJapanese,
  shouldSkip: (word) => !/^[぀-ゟ゠-ヿ＀-ﾟ一-龯㐀-䶿]+$/.test(word),
  normalize: (word) => word,
  phraseSeparator: '',
};

// Vietnamese is offered for dictionaries but has no splitter yet, so texts can't use it.
const SPLITTERS: Language[] = [english, chinese, japanese];

export function hasSplitter(code: string) {
  return SPLITTERS.some((l) => l.code === code);
}

export function getLanguage(code: string): Language {
  const language = SPLITTERS.find((l) => l.code === code);
  if (!language) throw new Error(`Language ${code} is not supported.`);
  return language;
}

/** terms.content is varchar(95); Postgres counts characters, not UTF-16 units. */
export const MAX_TERM_LENGTH = 95;

/** The terms-table key for a token, or null when it is skipped (punctuation, or too long to store). */
export function termKey(language: Language, word: string): string | null {
  if (language.shouldSkip(word)) return null;
  const key = language.normalize(word);
  return [...key].length > MAX_TERM_LENGTH ? null : key;
}

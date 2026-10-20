import type { Language } from './types';

// Same order as GET /api/language in the .NET API.
export const LANGUAGES: Language[] = [
  { name: 'English', code: 'en', speakCode: 'en-US', supportsTexts: true },
  { name: 'Chinese', code: 'zh', speakCode: 'zh-CN', supportsTexts: true },
  { name: 'Japanese', code: 'ja', speakCode: 'ja-JP', supportsTexts: true },
  // Only usable as a dictionary language until it has a splitter.
  { name: 'Vietnamese', code: 'vi', speakCode: 'vi_VN', supportsTexts: false },
];

export function languageName(code: string) {
  return LANGUAGES.find((l) => l.code === code)?.name ?? 'Unknown language';
}

export function isLanguageCode(code: string) {
  return LANGUAGES.some((l) => l.code === code);
}

export function isTextLanguageCode(code: string) {
  return LANGUAGES.some((l) => l.code === code && l.supportsTexts);
}

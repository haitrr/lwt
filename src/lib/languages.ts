import type { Language } from './types';

// Same order as GET /api/language in the .NET API.
export const LANGUAGES: Language[] = [
  { name: 'English', code: 'en', speakCode: 'en-US' },
  { name: 'Chinese', code: 'zh', speakCode: 'zh-CN' },
  { name: 'Japanese', code: 'ja', speakCode: 'ja-JP' },
  { name: 'Vietnamese', code: 'vi', speakCode: 'vi_VN' },
];

export function languageName(code: string) {
  return LANGUAGES.find((l) => l.code === code)?.name ?? 'Unknown language';
}

export function isLanguageCode(code: string) {
  return LANGUAGES.some((l) => l.code === code);
}

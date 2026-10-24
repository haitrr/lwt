import { extensionRoute } from '@/server/extensionApi';
import { LANGUAGES } from '@/lib/languages';
import { getLanguageSettings } from '@/server/users';

/** The signed-in user and the languages pages can be read in, with the dictionary language chosen for each. */
export const GET = extensionRoute(async (_req, session) => {
  const settings = await getLanguageSettings(session.userId);
  return {
    userName: session.userName,
    languages: LANGUAGES.filter((l) => l.supportsTexts).map(({ code, name, speakCode }) => ({
      code,
      name,
      speakCode,
      dictionaryLanguageCode: settings.find((s) => s.languageCode === code)?.dictionaryLanguageCode ?? null,
    })),
  };
});

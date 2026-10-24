import { extensionRoute, readJson } from '@/server/extensionApi';
import { makeWebPhrase } from '@/server/webReading';

/** Joins selected text ({ languageCode, text, learningLevel, meaning }) into one term everywhere. */
export const POST = extensionRoute(async (req, session) => makeWebPhrase(session, await readJson(req)));

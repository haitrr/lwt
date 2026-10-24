import { extensionRoute, readJson } from '@/server/extensionApi';
import { annotateWebText } from '@/server/webReading';

/** Splits a page's text ({ languageCode, segments }) into words with the user's terms for them. */
export const POST = extensionRoute(async (req, session) => annotateWebText(session, await readJson(req)));

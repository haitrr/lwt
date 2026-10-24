import { extensionRoute, readJson } from '@/server/extensionApi';
import { saveWebTerm } from '@/server/webReading';

/** Saves a word's level and meaning: { termId } or { languageCode, word }, with { learningLevel, meaning }. */
export const PUT = extensionRoute(async (req, session) => saveWebTerm(session, await readJson(req)));

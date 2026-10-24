import { extensionRoute, readJson } from '@/server/extensionApi';
import { splitWebPhrase } from '@/server/webReading';

/** Splits the phrase { termId } back into its words everywhere. */
export const POST = extensionRoute(async (req, session) => splitWebPhrase(session, await readJson(req)));

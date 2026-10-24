import { extensionRoute } from '@/server/extensionApi';
import { lookUpWebWord } from '@/server/webReading';

/** A dictionary meaning for ?word= in ?languageCode=. */
export const GET = extensionRoute(async (req, session) => {
  const { searchParams } = req.nextUrl;
  return lookUpWebWord(session, searchParams.get('languageCode') ?? '', searchParams.get('word') ?? '');
});

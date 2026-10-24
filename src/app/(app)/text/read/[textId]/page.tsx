import { notFound } from 'next/navigation';
import { parseId } from '@/lib/ids';
import { requireSession } from '@/server/auth/session';
import { UserError } from '@/server/errors';
import { getLanguage } from '@/server/languages';
import { getTermCounts, readText } from '@/server/texts';
import { getLanguageSettings } from '@/server/users';
import { LANGUAGES } from '@/lib/languages';
import Reader from './Reader';

export default async function TextReadPage({ params }: PageProps<'/text/read/[textId]'>) {
  const id = parseId((await params).textId);
  if (id === null) notFound();
  const session = await requireSession();
  const text = await readText(id, session).catch((e) => {
    if (e instanceof UserError) notFound();
    throw e;
  });
  const [counts, settings] = await Promise.all([getTermCounts(id, session.userId), getLanguageSettings(session.userId)]);
  const language = LANGUAGES.find((l) => l.code === text.languageCode);
  const dictionaryLanguage = settings.find((s) => s.languageCode === text.languageCode)?.dictionaryLanguageCode ?? null;
  return (
    <Reader
      text={text}
      initialCounts={counts}
      speakCode={language?.speakCode ?? null}
      dictionaryLanguage={dictionaryLanguage}
      phraseSeparator={getLanguage(text.languageCode).phraseSeparator}
    />
  );
}

import { notFound } from 'next/navigation';
import { LANGUAGES } from '@/lib/languages';
import { editText } from '@/lib/texts';
import { requireSession } from '@/server/auth/session';
import { UserError } from '@/server/errors';
import { getTextEditDetail } from '@/server/texts';
import TextForm from '../../TextForm';

export const metadata = { title: 'Edit text' };

export default async function EditTextPage({ params }: PageProps<'/text/[textId]/edit'>) {
  const id = Number((await params).textId);
  const session = await requireSession();
  const detail = await getTextEditDetail(id, session.userId).catch((e) => {
    if (e instanceof UserError) notFound();
    throw e;
  });
  return (
    <>
      <h1>Edit text</h1>
      <TextForm languages={LANGUAGES} action={editText.bind(null, id)} initial={detail} submitLabel="Save" />
    </>
  );
}

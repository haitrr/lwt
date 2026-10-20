import { LANGUAGES } from '@/lib/languages';
import { createText } from '@/lib/texts';
import TextForm from '../TextForm';

export const metadata = { title: 'Add text' };

export default function NewTextPage() {
  return (
    <>
      <h1 className="mb-4 text-2xl font-semibold tracking-tight">Add text</h1>
      <TextForm languages={LANGUAGES} action={createText} submitLabel="Add" />
    </>
  );
}

import { LANGUAGES } from '@/lib/languages';
import { createText } from '@/lib/texts';
import TextForm from '../TextForm';

export const metadata = { title: 'Add text' };

export default function NewTextPage() {
  return (
    <>
      <h1>Add text</h1>
      <TextForm languages={LANGUAGES} action={createText} submitLabel="Add" />
    </>
  );
}

import { LANGUAGES } from '@/lib/languages';
import { createText } from '@/lib/texts';
import TextForm from '../TextForm';

export const metadata = { title: 'Add text' };

export default function NewTextPage() {
  return (
    <>
      <div className="pageHeader">
        <h1>Add text</h1>
      </div>
      <TextForm languages={LANGUAGES} action={createText} submitLabel="Add" />
    </>
  );
}

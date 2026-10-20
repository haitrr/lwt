'use client';

import Link from 'next/link';
import { useActionState } from 'react';
import type { TextFormState } from '@/lib/texts';
import type { Language, TextEditDetail } from '@/lib/types';
import LanguageSelect from './LanguageSelect';
import styles from './texts.module.css';

type Props = {
  languages: Language[];
  action: (prev: TextFormState, formData: FormData) => Promise<TextFormState>;
  initial?: TextEditDetail;
  submitLabel: string;
};

export default function TextForm({ languages, action, initial, submitLabel }: Props) {
  const [state, formAction, pending] = useActionState(action, undefined);
  const values = state?.values ?? initial;
  return (
    <form action={formAction} className="card stack narrow">
      <div className={styles.formRow}>
        <label>
          Title
          <input name="title" placeholder="Title" defaultValue={values?.title} required />
        </label>
        <label>
          Language
          <LanguageSelect
            languages={languages}
            name="languageCode"
            defaultValue={values?.languageCode ?? ''}
            emptyLabel="Choose a language"
            required
          />
        </label>
      </div>
      <label>
        Content
        <textarea name="content" rows={16} placeholder="Text content" defaultValue={values?.content} required />
      </label>
      {state?.error && <p className="error">{state.error}</p>}
      <div className={styles.formActions}>
        <Link href="/text" className="button ghost">
          Cancel
        </Link>
        <button type="submit" disabled={pending}>
          {submitLabel}
        </button>
      </div>
    </form>
  );
}

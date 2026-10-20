'use client';

import { useActionState, useState } from 'react';
import { saveDictionarySettings } from '@/lib/profile';
import type { Language } from '@/lib/types';
import LanguageSelect from '../text/LanguageSelect';

type Setting = { languageCode: string; dictionaryLanguageCode: string };

export default function DictionarySettingsForm({ languages, settings }: { languages: Language[]; settings: Setting[] }) {
  const [rows, setRows] = useState<Setting[]>(settings);
  const [state, formAction, pending] = useActionState(saveDictionarySettings, undefined);
  const unused = languages.filter((l) => !rows.some((r) => r.languageCode === l.code));
  const nameOf = (code: string) => languages.find((l) => l.code === code)?.name ?? code;

  return (
    <form action={formAction} className="stack">
      {rows.map((row, i) => (
        <div className="row" key={row.languageCode}>
          <input type="hidden" name="languageCode" value={row.languageCode} />
          <span>{nameOf(row.languageCode)}</span>
          <LanguageSelect
            languages={languages}
            name="dictionaryLanguageCode"
            emptyLabel="Dictionary language"
            value={row.dictionaryLanguageCode}
            onChange={(e) =>
              setRows(rows.map((r, j) => (j === i ? { ...r, dictionaryLanguageCode: e.target.value } : r)))
            }
            required
          />
          <button type="button" className="danger" onClick={() => setRows(rows.filter((_, j) => j !== i))}>
            Remove
          </button>
        </div>
      ))}
      {state?.error && <p className="error">{state.error}</p>}
      {state?.values?.saved && <p>{state.values.saved}</p>}
      <div className="row">
        <button
          type="button"
          disabled={unused.length === 0}
          onClick={() => setRows([...rows, { languageCode: unused[0].code, dictionaryLanguageCode: 'en' }])}
        >
          Add {unused[0]?.name ?? 'language'}
        </button>
        <button type="submit" disabled={pending}>
          Save
        </button>
      </div>
    </form>
  );
}

'use client';

import { useActionState, useState } from 'react';
import { CircleAlert, CircleCheck, Plus, Trash2 } from 'lucide-react';
import { saveDictionarySettings } from '@/lib/profile';
import type { Language } from '@/lib/types';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import LanguageSelect from '../text/LanguageSelect';

type Setting = { languageCode: string; dictionaryLanguageCode: string };

export default function DictionarySettingsForm({ languages, settings }: { languages: Language[]; settings: Setting[] }) {
  const [rows, setRows] = useState<Setting[]>(settings);
  const [state, formAction, pending] = useActionState(saveDictionarySettings, undefined);
  const unused = languages.filter((l) => !rows.some((r) => r.languageCode === l.code));
  const nameOf = (code: string) => languages.find((l) => l.code === code)?.name ?? code;

  return (
    <form action={formAction} className="grid gap-4">
      {rows.length === 0 && <p className="text-sm text-muted-foreground">No languages set up yet.</p>}
      {rows.length > 0 && (
        <div className="divide-y rounded-md border">
          {rows.map((row, i) => (
            <div className="grid grid-cols-[1fr_auto] items-center gap-2 p-3 sm:grid-cols-[10rem_1fr_auto]" key={row.languageCode}>
              <input type="hidden" name="languageCode" value={row.languageCode} />
              <span className="text-sm font-medium">{nameOf(row.languageCode)}</span>
              <LanguageSelect
                languages={languages}
                name="dictionaryLanguageCode"
                emptyLabel="Dictionary language"
                aria-label={`Dictionary language for ${nameOf(row.languageCode)}`}
                className="col-span-2 row-start-2 w-full sm:col-span-1 sm:row-start-1"
                value={row.dictionaryLanguageCode}
                onChange={(e) =>
                  setRows(rows.map((r, j) => (j === i ? { ...r, dictionaryLanguageCode: e.target.value } : r)))
                }
                required
              />
              <Button
                type="button"
                variant="ghost"
                size="icon-sm"
                className="text-destructive hover:bg-destructive/10 hover:text-destructive"
                onClick={() => setRows(rows.filter((_, j) => j !== i))}
                aria-label={`Remove ${nameOf(row.languageCode)}`}
                title="Remove"
              >
                <Trash2 />
              </Button>
            </div>
          ))}
        </div>
      )}
      {state?.error && (
        <Alert variant="destructive">
          <CircleAlert />
          <AlertDescription>{state.error}</AlertDescription>
        </Alert>
      )}
      {state?.values?.saved && (
        <Alert>
          <CircleCheck />
          <AlertDescription>{state.values.saved}</AlertDescription>
        </Alert>
      )}
      <div className="flex justify-end gap-2">
        <Button
          type="button"
          variant="outline"
          disabled={unused.length === 0}
          onClick={() => setRows([...rows, { languageCode: unused[0].code, dictionaryLanguageCode: 'en' }])}
        >
          <Plus />
          Add {unused[0]?.name ?? 'language'}
        </Button>
        <Button type="submit" disabled={pending}>
          Save
        </Button>
      </div>
    </form>
  );
}

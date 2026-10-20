'use client';

import { useActionState } from 'react';
import { CircleAlert, CircleCheck } from 'lucide-react';
import { saveTranslator } from '@/lib/profile';
import type { TranslatorSettingsView } from '@/server/users';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

export default function TranslatorForm({ settings }: { settings: TranslatorSettingsView }) {
  const [state, formAction, pending] = useActionState(saveTranslator, undefined);
  return (
    <form action={formAction} className="grid gap-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="grid gap-2">
          <Label htmlFor="translatorKey">Key</Label>
          <Input
            id="translatorKey"
            name="key"
            type="password"
            autoComplete="off"
            placeholder={settings.hasKey ? 'Saved; type a new one to replace it' : 'Azure Translator key'}
            required={!settings.hasKey}
          />
        </div>
        <div className="grid gap-2">
          <Label htmlFor="translatorRegion">Region</Label>
          <Input
            id="translatorRegion"
            name="region"
            placeholder="e.g. eastasia; empty for a global resource"
            defaultValue={state?.values?.region ?? settings.region}
          />
        </div>
      </div>
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
        {settings.hasKey && (
          <Button
            type="submit"
            name="intent"
            value="remove"
            variant="outline"
            className="text-destructive hover:bg-destructive/10 hover:text-destructive"
            formNoValidate
            disabled={pending}
          >
            Remove key
          </Button>
        )}
        <Button type="submit" disabled={pending}>
          Save
        </Button>
      </div>
    </form>
  );
}

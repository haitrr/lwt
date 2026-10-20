'use client';

import Link from 'next/link';
import { useActionState } from 'react';
import { CircleAlert } from 'lucide-react';
import type { TextFormState } from '@/lib/texts';
import type { Language, TextEditDetail } from '@/lib/types';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import LanguageSelect from './LanguageSelect';

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
    <Card className="max-w-3xl">
      <form action={formAction} className="contents">
        <CardContent className="grid gap-5">
          <div className="grid gap-5 sm:grid-cols-[2fr_1fr]">
            <div className="grid gap-2">
              <Label htmlFor="title">Title</Label>
              <Input id="title" name="title" placeholder="Title" defaultValue={values?.title} required />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="languageCode">Language</Label>
              <LanguageSelect
                id="languageCode"
                languages={languages}
                textsOnly
                name="languageCode"
                defaultValue={values?.languageCode ?? ''}
                emptyLabel="Choose a language"
                className="w-full"
                required
              />
            </div>
          </div>
          <div className="grid gap-2">
            <Label htmlFor="content">Content</Label>
            <Textarea
              id="content"
              name="content"
              rows={16}
              placeholder="Text content"
              className="min-h-80"
              defaultValue={values?.content}
              required
            />
          </div>
          {state?.error && (
            <Alert variant="destructive">
              <CircleAlert />
              <AlertDescription>{state.error}</AlertDescription>
            </Alert>
          )}
        </CardContent>
        <CardFooter className="justify-end gap-2">
          <Button asChild variant="ghost">
            <Link href="/text">Cancel</Link>
          </Button>
          <Button type="submit" disabled={pending}>
            {submitLabel}
          </Button>
        </CardFooter>
      </form>
    </Card>
  );
}

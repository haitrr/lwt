'use client';

import Link from 'next/link';
import { useActionState } from 'react';
import { CircleAlert } from 'lucide-react';
import type { AuthState } from '@/lib/auth';
import Brand from '@/components/Brand';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

type Props = {
  title: string;
  subtitle: string;
  submitLabel: string;
  action: (prev: AuthState, formData: FormData) => Promise<AuthState>;
  alternate: { prompt: string; href: string; label: string };
  newPassword?: boolean;
};

export default function AuthForm({ title, subtitle, submitLabel, action, alternate, newPassword }: Props) {
  const [state, formAction, pending] = useActionState(action, undefined);
  return (
    <main className="flex min-h-dvh flex-col items-center justify-center gap-6 bg-muted p-4">
      <Brand className="text-lg" />
      <Card className="w-full max-w-sm">
        <CardHeader>
          <CardTitle className="text-xl">{title}</CardTitle>
          <CardDescription>{subtitle}</CardDescription>
        </CardHeader>
        <CardContent>
          <form action={formAction} className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="userName">Username</Label>
              <Input
                id="userName"
                name="userName"
                autoComplete="username"
                defaultValue={state?.values?.userName}
                required
                autoFocus
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                name="password"
                type="password"
                autoComplete={newPassword ? 'new-password' : 'current-password'}
                required
              />
            </div>
            {state?.error && (
              <Alert variant="destructive">
                <CircleAlert />
                <AlertDescription>{state.error}</AlertDescription>
              </Alert>
            )}
            <Button type="submit" className="w-full" disabled={pending}>
              {pending ? 'Please wait…' : submitLabel}
            </Button>
          </form>
        </CardContent>
        <CardFooter className="justify-center text-sm text-muted-foreground">
          {alternate.prompt}&nbsp;
          <Link href={alternate.href} className="font-medium text-foreground underline underline-offset-4">
            {alternate.label}
          </Link>
        </CardFooter>
      </Card>
    </main>
  );
}

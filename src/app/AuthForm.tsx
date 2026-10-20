'use client';

import Link from 'next/link';
import { useActionState } from 'react';
import type { AuthState } from '@/lib/auth';
import Brand from '@/components/Brand';

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
    <main className="auth">
      <div className="authCard">
        <Brand />
        <h1>{title}</h1>
        <p className="subtle" style={{ margin: 0 }}>
          {subtitle}
        </p>
        <form action={formAction}>
          <label>
            Username
            <input name="userName" autoComplete="username" defaultValue={state?.values?.userName} required autoFocus />
          </label>
          <label>
            Password
            <input
              name="password"
              type="password"
              autoComplete={newPassword ? 'new-password' : 'current-password'}
              required
            />
          </label>
          {state?.error && <p className="error">{state.error}</p>}
          <button type="submit" className="button" disabled={pending}>
            {pending ? 'Please wait…' : submitLabel}
          </button>
        </form>
        <p className="authAlt">
          {alternate.prompt} <Link href={alternate.href}>{alternate.label}</Link>
        </p>
      </div>
    </main>
  );
}

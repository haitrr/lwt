'use client';

import Link from 'next/link';
import { useActionState } from 'react';
import type { AuthState } from '@/lib/auth';

type Props = {
  title: string;
  action: (prev: AuthState, formData: FormData) => Promise<AuthState>;
  alternate: { href: string; label: string };
};

export default function AuthForm({ title, action, alternate }: Props) {
  const [state, formAction, pending] = useActionState(action, undefined);
  return (
    <main className="auth">
      <h1>{title}</h1>
      <form action={formAction}>
        <label>
          Username
          <input name="userName" autoComplete="username" defaultValue={state?.values?.userName} required />
        </label>
        <label>
          Password
          <input name="password" type="password" autoComplete="current-password" required />
        </label>
        {state?.error && <p className="error">{state.error}</p>}
        <button type="submit" disabled={pending}>
          {title}
        </button>
      </form>
      <Link href={alternate.href}>{alternate.label}</Link>
    </main>
  );
}

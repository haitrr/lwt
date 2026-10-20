'use client';

import { useActionState } from 'react';
import { changePassword } from '@/lib/profile';

export default function PasswordForm() {
  const [state, formAction, pending] = useActionState(changePassword, undefined);
  return (
    <form action={formAction} className="stack" style={{ maxWidth: '20rem' }}>
      <input name="currentPassword" type="password" placeholder="Current password" autoComplete="current-password" required />
      <input name="newPassword" type="password" placeholder="New password" autoComplete="new-password" required />
      {state?.error && <p className="error">{state.error}</p>}
      {state?.values?.saved && <p>{state.values.saved}</p>}
      <button type="submit" disabled={pending}>
        Change password
      </button>
    </form>
  );
}

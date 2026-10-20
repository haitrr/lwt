'use client';

import { useActionState } from 'react';
import { changePassword } from '@/lib/profile';
import styles from './profile.module.css';

export default function PasswordForm() {
  const [state, formAction, pending] = useActionState(changePassword, undefined);
  return (
    <form action={formAction} className="stack">
      <div className={styles.passwordFields}>
        <label>
          Current password
          <input name="currentPassword" type="password" autoComplete="current-password" required />
        </label>
        <label>
          New password
          <input name="newPassword" type="password" autoComplete="new-password" required />
        </label>
      </div>
      {state?.error && <p className="error">{state.error}</p>}
      {state?.values?.saved && <p className="success">{state.values.saved}</p>}
      <div className={styles.actions}>
        <button type="submit" disabled={pending}>
          Change password
        </button>
      </div>
    </form>
  );
}

'use server';

import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import { toFormState, type FormState } from './actionState';
import { TOKEN_COOKIE } from './config';
import { createSessionToken, setSessionCookie } from '@/server/auth/session';
import { checkCredentials, signUp } from '@/server/users';

export type AuthState = FormState;

function readCredentials(formData: FormData) {
  return {
    userName: String(formData.get('userName') ?? ''),
    password: String(formData.get('password') ?? ''),
  };
}

export async function login(_prev: AuthState, formData: FormData): Promise<AuthState> {
  const { userName, password } = readCredentials(formData);
  const state = await toFormState(async () => {
    const user = await checkCredentials(userName, password);
    await setSessionCookie(await createSessionToken(user.id, user.userName));
  }, { userName });
  if (state) return state;
  redirect('/');
}

export async function register(_prev: AuthState, formData: FormData): Promise<AuthState> {
  const { userName, password } = readCredentials(formData);
  const state = await toFormState(() => signUp(userName, password), { userName });
  if (state) return state;
  redirect('/login');
}

export async function logout() {
  (await cookies()).delete(TOKEN_COOKIE);
  redirect('/login');
}

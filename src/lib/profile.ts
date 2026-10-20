'use server';

import { revalidatePath } from 'next/cache';
import { toFormState, type FormState } from './actionState';
import { requireSession } from '@/server/auth/session';
import { changePassword as changeUserPassword, saveLanguageSettings } from '@/server/users';

export async function saveDictionarySettings(_prev: FormState, formData: FormData): Promise<FormState> {
  const { userId } = await requireSession();
  const languageCodes = formData.getAll('languageCode').map(String);
  const dictionaryCodes = formData.getAll('dictionaryLanguageCode').map(String);
  const settings = languageCodes.map((languageCode, i) => ({ languageCode, dictionaryLanguageCode: dictionaryCodes[i] }));
  const state = await toFormState(() => saveLanguageSettings(userId, settings));
  revalidatePath('/profile');
  return state ?? { values: { saved: 'Saved.' } };
}

export async function changePassword(_prev: FormState, formData: FormData): Promise<FormState> {
  const { userId } = await requireSession();
  const current = String(formData.get('currentPassword') ?? '');
  const next = String(formData.get('newPassword') ?? '');
  const state = await toFormState(() => changeUserPassword(userId, current, next));
  return state ?? { values: { saved: 'Password changed.' } };
}

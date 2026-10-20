'use server';

import { revalidatePath } from 'next/cache';
import { redirect } from 'next/navigation';
import { toFormState, type FormState } from './actionState';
import { requireSession } from '@/server/auth/session';
import { processNextText } from '@/server/processing';
import * as texts from '@/server/texts';

export type TextFormState = FormState;

function readText(formData: FormData) {
  return {
    title: String(formData.get('title') ?? ''),
    languageCode: String(formData.get('languageCode') ?? ''),
    // Form posts send textarea line breaks as \r\n; the old app stored the textarea value (\n).
    content: String(formData.get('content') ?? '').replace(/\r\n/g, '\n'),
  };
}

export async function createText(_prev: TextFormState, formData: FormData): Promise<TextFormState> {
  const session = await requireSession();
  const input = readText(formData);
  const state = await toFormState(() => texts.createText(session, input), input);
  if (state) return state;
  redirect('/text');
}

export async function editText(id: number, _prev: TextFormState, formData: FormData): Promise<TextFormState> {
  const session = await requireSession();
  const input = readText(formData);
  const state = await toFormState(() => texts.editText(id, session.userId, input), input);
  if (state) return state;
  redirect('/text');
}

export async function deleteText(id: number) {
  const session = await requireSession();
  await texts.deleteText(id, session.userId);
  revalidatePath('/text');
}

/**
 * Works through pending text splitting for a few seconds. The background loop does this on a
 * long-running server; on serverless hosts (Vercel) there is no such loop, so pages that wait
 * on processing call this instead.
 */
export async function advanceProcessing() {
  await requireSession();
  const deadline = Date.now() + 3000;
  while (Date.now() < deadline && (await processNextText())) {
    // keep going
  }
}

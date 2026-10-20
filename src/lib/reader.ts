'use server';

import { requireSession } from '@/server/auth/session';
import { lookUpMeaning } from '@/server/dictionary';
import { UserError } from '@/server/errors';
import { editTerm } from '@/server/terms';
import { getTermCounts, getTextTerms, setBookmark } from '@/server/texts';
import { getTranslatorSettings } from '@/server/users';
import type { LearningLevel } from './types';

// Server functions the reader calls from the browser. Each checks the session itself.

export async function loadTextTerms(textId: number, indexFrom: number, indexTo: number) {
  const { userId } = await requireSession();
  return getTextTerms(textId, userId, indexFrom, indexTo);
}

export async function loadTermCounts(textId: number) {
  const { userId } = await requireSession();
  return getTermCounts(textId, userId);
}

export async function saveBookmark(textId: number, index: number) {
  const { userId } = await requireSession();
  await setBookmark(textId, userId, index);
}

export async function saveTerm(termId: number, learningLevel: LearningLevel, meaning: string) {
  const session = await requireSession();
  try {
    await editTerm(session, termId, { learningLevel, meaning });
    return { ok: true as const };
  } catch (e) {
    if (e instanceof UserError) return { ok: false as const, error: e.message };
    throw e;
  }
}

export async function lookUpTerm(word: string, from: string, to: string) {
  const { userId } = await requireSession();
  return lookUpMeaning(word, from, to, await getTranslatorSettings(userId));
}

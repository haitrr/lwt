'use server';

import { requireSession } from '@/server/auth/session';
import { lookUpMeaning } from '@/server/dictionary';
import { UserError } from '@/server/errors';
import { findPhrase, joinPhraseEverywhere, makePhrase, splitPhrase } from '@/server/phrases';
import { editTerm } from '@/server/terms';
import { getTermCounts, getTextProgress, getTextTerms, setBookmark } from '@/server/texts';
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

export async function loadTextProgress(textId: number) {
  const { userId } = await requireSession();
  return getTextProgress(textId, userId);
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

async function userAction(action: () => Promise<unknown>) {
  try {
    await action();
    return { ok: true as const };
  } catch (e) {
    if (e instanceof UserError) return { ok: false as const, error: e.message };
    throw e;
  }
}

/** Makes the words from token `from` to `to` one term, joined here only or in all the user's texts. */
export async function savePhrase(
  textId: number,
  from: number,
  to: number,
  learningLevel: LearningLevel,
  meaning: string,
  everywhere: boolean,
) {
  const session = await requireSession();
  return userAction(() => makePhrase(session, { textId, from, to, learningLevel, meaning, everywhere }));
}

/** The saved phrase the words from token `from` to `to` spell, or null when there is none or they can't form one. */
export async function loadPhrase(textId: number, from: number, to: number) {
  const session = await requireSession();
  try {
    const term = await findPhrase(session, { textId, from, to });
    return term && { ...term, learningLevel: term.learningLevel as LearningLevel };
  } catch (e) {
    if (e instanceof UserError) return null;
    throw e;
  }
}

/** Joins a phrase in every text where its words appear, new texts included. */
export async function joinEverywhere(termId: number) {
  const session = await requireSession();
  return userAction(() => joinPhraseEverywhere(session, termId));
}

/** Turns a phrase back into separate words at one occurrence (`at`), or everywhere when at is null. */
export async function unmakePhrase(termId: number, at: { textId: number; index: number } | null) {
  const session = await requireSession();
  return userAction(() => splitPhrase(session, termId, at));
}

export async function lookUpTerm(word: string, from: string, to: string) {
  const { userId } = await requireSession();
  return lookUpMeaning(word, from, to, await getTranslatorSettings(userId));
}

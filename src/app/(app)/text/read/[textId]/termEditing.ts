/**
 * What the term panel shows and saves. `edited` is null until the user types, so a box they
 * cleared stays empty instead of falling back to the stored or suggested meaning.
 */
export function panelMeaning(edited: string | null, stored: string | null, suggestion: string | null) {
  return edited ?? (stored || suggestion || '');
}

export const SAVE_FAILED = 'Could not save. Check your connection and try again.';

/** Null when the save went through, otherwise the message to show next to the term. */
export async function saveError(save: Promise<{ ok: true } | { ok: false; error: string }>) {
  try {
    const result = await save;
    return result.ok ? null : result.error;
  } catch {
    return SAVE_FAILED;
  }
}

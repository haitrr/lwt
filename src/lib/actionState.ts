import 'server-only';
import { UserError } from '@/server/errors';

// `values` are echoed back so the form can show what was typed: React resets forms after an action.
export type FormState = { error?: string; values?: Record<string, string> } | undefined;

/** Runs a mutation and turns expected failures into a message for the form. */
export async function toFormState(fn: () => Promise<unknown>, values?: Record<string, string>): Promise<FormState> {
  try {
    await fn();
    return undefined;
  } catch (e) {
    if (e instanceof UserError) return { error: e.message, values };
    console.error(e);
    return { error: 'Something went wrong, please try again.', values };
  }
}

import { describe, expect, it } from 'vitest';
import { panelMeaning, SAVE_FAILED, saveError } from './termEditing';

describe('panelMeaning', () => {
  it('shows the stored meaning, then the suggestion, until the user edits', () => {
    expect(panelMeaning(null, 'stored', 'suggested')).toBe('stored');
    expect(panelMeaning(null, null, 'suggested')).toBe('suggested');
    expect(panelMeaning(null, '', 'suggested')).toBe('suggested');
    expect(panelMeaning(null, null, null)).toBe('');
  });

  it('keeps what the user typed, including an empty box', () => {
    expect(panelMeaning('mine', 'stored', 'suggested')).toBe('mine');
    expect(panelMeaning('', 'stored', 'suggested')).toBe('');
    expect(panelMeaning('', null, 'suggested')).toBe('');
  });
});

describe('saveError', () => {
  it('is null when the save went through', async () => {
    expect(await saveError(Promise.resolve({ ok: true }))).toBeNull();
  });

  it('passes on the server message', async () => {
    expect(await saveError(Promise.resolve({ ok: false, error: 'term not found' }))).toBe('term not found');
  });

  it('turns a thrown or network error into a message instead of rejecting', async () => {
    expect(await saveError(Promise.reject(new TypeError('Failed to fetch')))).toBe(SAVE_FAILED);
  });
});

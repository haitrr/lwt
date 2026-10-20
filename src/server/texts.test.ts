import { describe, expect, it, vi } from 'vitest';

vi.mock('server-only', () => ({}));
vi.mock('./db', async () => {
  const { schema } = await vi.importActual<typeof import('./db')>('./db');
  return {
    schema,
    getDb: () => {
      throw new Error('validation should reject before touching the database');
    },
  };
});
const { createText, editText } = await import('./texts');
const { UserError } = await import('./errors');

const session = { userId: 1, userName: 'reader' } as Parameters<typeof createText>[0];
const input = { title: 'Xin chào', content: 'Tôi là sinh viên.' };

describe('text validation', () => {
  it('rejects languages without a splitter', async () => {
    await expect(createText(session, { ...input, languageCode: 'vi' })).rejects.toThrow(UserError);
    await expect(editText(1, 1, { ...input, languageCode: 'vi' })).rejects.toThrow('Unsupported language.');
  });

  it('accepts languages with a splitter', async () => {
    await expect(createText(session, { ...input, languageCode: 'en' })).rejects.toThrow('validation should reject');
  });
});

import { beforeEach, describe, expect, it, vi } from 'vitest';
import { UserError } from './errors';

let transaction: () => Promise<void>;
let found: { id: number; userName: string; passwordHash: string; tokenVersion: number }[];

vi.mock('server-only', () => ({}));
vi.mock('./db', async () => {
  const { schema } = await vi.importActual<typeof import('./db')>('./db');
  return {
    schema,
    getDb: () => ({
      transaction: () => transaction(),
      select: () => ({ from: () => ({ where: async () => found }) }),
    }),
  };
});
vi.mock('./auth/password', async (importOriginal) => {
  const actual = await importOriginal<typeof import('./auth/password')>();
  return { ...actual, verifyDummyPassword: vi.fn(actual.verifyDummyPassword), verifyPassword: vi.fn(actual.verifyPassword) };
});
const password = await import('./auth/password');
const { checkCredentials, signUp } = await import('./users');

describe('signUp', () => {
  beforeEach(() => {
    transaction = async () => {};
  });

  it('requires new passwords to be at least 8 characters', async () => {
    await expect(signUp('ann', '')).rejects.toThrow('Password is required.');
    await expect(signUp('ann', '1234567')).rejects.toThrow('at least 8 characters');
    await expect(signUp('ann', '12345678')).resolves.toBeUndefined();
  });

  it('reports a taken user name when the unique index catches a race', async () => {
    transaction = async () => {
      throw Object.assign(new Error('Failed query'), { cause: Object.assign(new Error('duplicate key'), { code: '23505' }) });
    };
    const error = await signUp('ann', 'long enough').catch((e) => e);
    expect(error).toBeInstanceOf(UserError);
    expect(error.message).toBe("Username 'ann' is already taken.");
  });

  it('passes other database errors on', async () => {
    transaction = async () => {
      throw Object.assign(new Error('connection refused'), { code: 'ECONNREFUSED' });
    };
    await expect(signUp('ann', 'long enough')).rejects.not.toBeInstanceOf(UserError);
  });
});

describe('checkCredentials', () => {
  beforeEach(() => vi.clearAllMocks());

  it('hashes the password even when the user does not exist', async () => {
    found = [];
    await expect(checkCredentials('nobody', 'secret')).rejects.toThrow('Username or password is incorrect.');
    expect(password.verifyDummyPassword).toHaveBeenCalledWith('secret');
  });

  it('accepts the right password, keeping short passwords from before the minimum', async () => {
    found = [{ id: 7, userName: 'ann', passwordHash: await password.hashPassword('q'), tokenVersion: 2 }];
    await expect(checkCredentials('ANN ', 'q')).resolves.toEqual({ id: 7, userName: 'ann', tokenVersion: 2 });
    await expect(checkCredentials('ann', 'r')).rejects.toThrow('Username or password is incorrect.');
    expect(password.verifyDummyPassword).not.toHaveBeenCalled();
  });
});

import { SignJWT } from 'jose';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const SECRET = 'a-test-secret-that-is-long-enough!!';
let cookie: string | undefined;
let storedVersion: number | undefined;

vi.mock('server-only', () => ({}));
vi.mock('next/headers', () => ({ cookies: async () => ({ get: () => (cookie ? { value: cookie } : undefined) }) }));
vi.mock('../db', async () => {
  const { schema } = await vi.importActual<typeof import('../db')>('../db');
  const rows = () => (storedVersion === undefined ? [] : [{ tokenVersion: storedVersion }]);
  return { schema, getDb: () => ({ select: () => ({ from: () => ({ where: async () => rows() }) }) }) };
});
vi.stubEnv('LWT_JWT_SECRET', SECRET);
const { createSessionToken, getSession, verifySessionToken } = await import('./session');

/** A token as the .NET API issued them: no tokenVersion claim. */
function dotnetToken() {
  return new SignJWT({ id: '7', userName: 'ann' })
    .setProtectedHeader({ alg: 'HS256', typ: 'JWT' })
    .setIssuedAt()
    .setExpirationTime('90d')
    .sign(new TextEncoder().encode(SECRET));
}

describe('session tokens', () => {
  beforeEach(() => {
    cookie = undefined;
    storedVersion = 0;
  });

  it('round-trips the token version', async () => {
    const token = await createSessionToken(7, 'ann', 3);
    expect(await verifySessionToken(token)).toEqual({ userId: 7, userName: 'ann', tokenVersion: 3 });
  });

  it('reads .NET tokens as version 0', async () => {
    expect(await verifySessionToken(await dotnetToken())).toEqual({ userId: 7, userName: 'ann', tokenVersion: 0 });
  });

  it('rejects tokens signed with another secret', async () => {
    const token = await new SignJWT({ id: '7', userName: 'ann' })
      .setProtectedHeader({ alg: 'HS256' })
      .sign(new TextEncoder().encode('another-secret-that-is-long-enough!'));
    expect(await verifySessionToken(token)).toBeNull();
  });

  it('accepts a token whose version matches the user', async () => {
    cookie = await dotnetToken();
    expect(await getSession()).toEqual({ userId: 7, userName: 'ann' });
  });

  it('rejects a token after the user’s version moved on', async () => {
    cookie = await dotnetToken();
    storedVersion = 1;
    expect(await getSession()).toBeNull();
    cookie = await createSessionToken(7, 'ann', 1);
    expect(await getSession()).toEqual({ userId: 7, userName: 'ann' });
  });

  it('rejects a token for a deleted user', async () => {
    cookie = await createSessionToken(7, 'ann', 0);
    storedVersion = undefined;
    expect(await getSession()).toBeNull();
  });
});

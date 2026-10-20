import 'server-only';
import { eq } from 'drizzle-orm';
import { jwtVerify, SignJWT } from 'jose';
import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import { cache } from 'react';
import { TOKEN_COOKIE } from '@/lib/config';
import { getDb, schema } from '../db';
import { jwtSecret } from './secret';

// Same token shape the .NET API issued (HS256, claims "id" and "userName", 3 months),
// so the secret can be shared with it during the switch-over. "tokenVersion" is ours: a token is only good while it
// matches users.token_version, which a password change bumps. The .NET tokens don't have it and count as version 0.

export type Session = { userId: number; userName: string };
export type SessionToken = Session & { tokenVersion: number };

export async function createSessionToken(userId: number, userName: string, tokenVersion: number) {
  return new SignJWT({ id: String(userId), userName, tokenVersion })
    .setProtectedHeader({ alg: 'HS256', typ: 'JWT' })
    .setIssuedAt()
    .setNotBefore('0s')
    .setExpirationTime('90d')
    .sign(jwtSecret());
}

/** The token's claims when its signature and dates check out; doesn't look at the database. */
export async function verifySessionToken(token: string): Promise<SessionToken | null> {
  try {
    const { payload } = await jwtVerify(token, jwtSecret(), { algorithms: ['HS256'] });
    const userId = Number(payload.id);
    const tokenVersion = payload.tokenVersion ?? 0;
    if (!Number.isInteger(userId) || typeof payload.userName !== 'string' || !Number.isInteger(tokenVersion)) {
      return null;
    }
    return { userId, userName: payload.userName, tokenVersion: tokenVersion as number };
  } catch {
    return null;
  }
}

/** The signed-in user, or null when there is no token or it was revoked. Looked up once per request. */
export const getSession = cache(async (): Promise<Session | null> => {
  const token = (await cookies()).get(TOKEN_COOKIE)?.value;
  const claims = token ? await verifySessionToken(token) : null;
  if (!claims) return null;
  const [user] = await getDb()
    .select({ tokenVersion: schema.users.tokenVersion })
    .from(schema.users)
    .where(eq(schema.users.id, claims.userId));
  if (user?.tokenVersion !== claims.tokenVersion) return null;
  return { userId: claims.userId, userName: claims.userName };
});

/** The signed-in user, or a redirect to /login. */
export async function requireSession(): Promise<Session> {
  const session = await getSession();
  if (!session) redirect('/login');
  return session;
}

export async function setSessionCookie(token: string) {
  (await cookies()).set(TOKEN_COOKIE, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    path: '/',
    maxAge: 90 * 24 * 60 * 60,
  });
}

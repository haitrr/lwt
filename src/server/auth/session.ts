import 'server-only';
import { jwtVerify, SignJWT } from 'jose';
import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import { TOKEN_COOKIE } from '@/lib/config';

// Same token shape the .NET API issued (HS256, claims "id" and "userName", 3 months),
// so the secret can be shared with it during the switch-over.

export type Session = { userId: number; userName: string };

function secret() {
  const value = process.env.LWT_JWT_SECRET;
  if (!value) throw new Error('LWT_JWT_SECRET is not set');
  return new TextEncoder().encode(value);
}

export async function createSessionToken(userId: number, userName: string) {
  return new SignJWT({ id: String(userId), userName })
    .setProtectedHeader({ alg: 'HS256', typ: 'JWT' })
    .setIssuedAt()
    .setNotBefore('0s')
    .setExpirationTime('90d')
    .sign(secret());
}

export async function verifySessionToken(token: string): Promise<Session | null> {
  try {
    const { payload } = await jwtVerify(token, secret(), { algorithms: ['HS256'] });
    const userId = Number(payload.id);
    if (!Number.isInteger(userId) || typeof payload.userName !== 'string') return null;
    return { userId, userName: payload.userName };
  } catch {
    return null;
  }
}

export async function getSession(): Promise<Session | null> {
  const token = (await cookies()).get(TOKEN_COOKIE)?.value;
  return token ? verifySessionToken(token) : null;
}

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

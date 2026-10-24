import type { NextRequest } from 'next/server';
import { clearLoginAttempts, clientIp, limitAttempt } from '@/server/auth/attempts';
import { createSessionToken } from '@/server/auth/session';
import { errorResponse, readJson } from '@/server/extensionApi';
import { checkCredentials } from '@/server/users';

/** Signs the extension in: the same token the login page puts in the cookie, handed over for a bearer header. */
export async function POST(req: NextRequest) {
  try {
    const body = await readJson(req);
    const userName = String(body.userName ?? '');
    limitAttempt('login', clientIp(req.headers), userName);
    const user = await checkCredentials(userName, String(body.password ?? ''));
    clearLoginAttempts(userName);
    return Response.json({ token: await createSessionToken(user.id, user.userName, user.tokenVersion), userName: user.userName });
  } catch (e) {
    return errorResponse(e);
  }
}

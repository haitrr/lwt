import 'server-only';
import type { NextRequest } from 'next/server';
import { getBearerSession, type Session } from './auth/session';
import { UserError } from './errors';

// JSON routes under /api/ext for the browser extension. It signs in with a bearer token rather than the cookie, so
// these can't be called by a page the user happens to visit (no CSRF), and they answer with { error } instead of
// redirecting to /login.

export function errorResponse(e: unknown) {
  if (e instanceof UserError) return Response.json({ error: e.message }, { status: e.status });
  throw e;
}

export async function readJson(req: NextRequest): Promise<Record<string, unknown>> {
  try {
    const body: unknown = await req.json();
    if (body && typeof body === 'object' && !Array.isArray(body)) return body as Record<string, unknown>;
  } catch {}
  throw new UserError('Expected a JSON object.');
}

/** A route handler that needs a signed-in user and answers with what `handler` returns, as JSON. */
export function extensionRoute(handler: (req: NextRequest, session: Session) => Promise<unknown>) {
  return async (req: NextRequest) => {
    const session = await getBearerSession(req.headers);
    if (!session) return Response.json({ error: 'Sign in to LWT again.' }, { status: 401 });
    try {
      return Response.json(await handler(req, session));
    } catch (e) {
      return errorResponse(e);
    }
  };
}

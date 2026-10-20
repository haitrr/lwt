import 'server-only';
import { UserError } from '../errors';
import { AttemptLimiter } from './rateLimit';

const MINUTE = 60_000;

function createLimiters() {
  return {
    login: {
      ip: new AttemptLimiter({ max: 30, windowMs: 15 * MINUTE }),
      userName: new AttemptLimiter({ max: 10, windowMs: 15 * MINUTE }),
    },
    register: {
      ip: new AttemptLimiter({ max: 10, windowMs: 60 * MINUTE }),
      userName: new AttemptLimiter({ max: 5, windowMs: 60 * MINUTE }),
    },
  };
}

declare global {
  var lwtAuthLimiters: ReturnType<typeof createLimiters> | undefined;
}

// Kept on globalThis so dev hot reloads don't reset the counts.
const limiters = (globalThis.lwtAuthLimiters ??= createLimiters());

/**
 * The client's address as the nearest proxy saw it: the last X-Forwarded-For entry, which Next.js fills in from the
 * socket when no proxy sent one.
 */
export function clientIp(headers: Headers): string {
  const forwarded = headers.get('x-forwarded-for')?.split(',').at(-1)?.trim();
  return forwarded || 'unknown';
}

/** Counts a sign-in or sign-up attempt, throwing when the address or the user name has made too many. */
export function limitAttempt(kind: 'login' | 'register', ip: string, userName: string) {
  const { ip: byIp, userName: byUserName } = limiters[kind];
  const wait = byIp.consume(ip) || byUserName.consume(userName.trim().toUpperCase());
  if (wait) {
    const minutes = Math.ceil(wait / MINUTE);
    throw new UserError(`Too many attempts. Try again in ${minutes} minute${minutes === 1 ? '' : 's'}.`);
  }
}

/** Forgets the failed sign-ins for a user name once someone gets its password right. */
export function clearLoginAttempts(userName: string) {
  limiters.login.userName.reset(userName.trim().toUpperCase());
}

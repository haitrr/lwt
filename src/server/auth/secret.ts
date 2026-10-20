// No 'server-only' here: instrumentation.ts checks the secret at startup.

/** The LWT_JWT_SECRET value in .env.example. */
export const PLACEHOLDER_SECRET = 'change-me-to-a-long-random-string';
/** HS256 needs a key at least as long as its 256-bit hash to be as strong as it. */
export const MIN_SECRET_BYTES = 32;

/** The key session tokens are signed with. In production a short or placeholder secret is an error. */
export function jwtSecret(env: Record<string, string | undefined> = process.env): Uint8Array {
  const value = env.LWT_JWT_SECRET;
  if (!value) throw new Error('LWT_JWT_SECRET is not set');
  const key = new TextEncoder().encode(value);
  if (env.NODE_ENV === 'production') {
    if (value === PLACEHOLDER_SECRET) {
      throw new Error('LWT_JWT_SECRET is still the .env.example placeholder; set it to a long random string');
    }
    if (key.length < MIN_SECRET_BYTES) {
      throw new Error(`LWT_JWT_SECRET must be at least ${MIN_SECRET_BYTES} bytes, e.g. from: openssl rand -base64 48`);
    }
  }
  return key;
}

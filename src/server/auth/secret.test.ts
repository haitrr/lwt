import { describe, expect, it } from 'vitest';
import { jwtSecret, PLACEHOLDER_SECRET } from './secret';

const strong = 'x'.repeat(32);

describe('jwtSecret', () => {
  it('requires a secret', () => {
    expect(() => jwtSecret({})).toThrow('not set');
  });

  it('refuses short or placeholder secrets in production', () => {
    expect(() => jwtSecret({ NODE_ENV: 'production', LWT_JWT_SECRET: 'x'.repeat(31) })).toThrow('at least 32 bytes');
    expect(() => jwtSecret({ NODE_ENV: 'production', LWT_JWT_SECRET: PLACEHOLDER_SECRET })).toThrow('placeholder');
    expect(jwtSecret({ NODE_ENV: 'production', LWT_JWT_SECRET: strong })).toEqual(new TextEncoder().encode(strong));
  });

  it('counts bytes, not characters', () => {
    expect(jwtSecret({ NODE_ENV: 'production', LWT_JWT_SECRET: 'é'.repeat(16) })).toHaveLength(32);
  });

  it('accepts any secret in development', () => {
    expect(jwtSecret({ NODE_ENV: 'development', LWT_JWT_SECRET: 'short' })).toHaveLength(5);
  });
});

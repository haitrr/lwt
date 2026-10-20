import { describe, expect, it } from 'vitest';
import { hashPassword, verifyPassword } from './password';

describe('ASP.NET Identity password hashes', () => {
  // Created by the .NET API (Identity V3, HMAC-SHA256, 10k iterations) for the password "q".
  const dotnetHash = 'AQAAAAEAACcQAAAAEBHYa2WhARafxqD3RGYCyMXqnmZK7h6yyzkTM3YHClO4IDTH4vS84GM8o5RdG9+J4Q==';

  it('verifies hashes made by the .NET API', () => {
    expect(verifyPassword('q', dotnetHash)).toBe(true);
    expect(verifyPassword('Q', dotnetHash)).toBe(false);
  });

  it('round-trips new hashes', () => {
    const hash = hashPassword('correct horse');
    expect(Buffer.from(hash, 'base64')[0]).toBe(0x01);
    expect(verifyPassword('correct horse', hash)).toBe(true);
    expect(verifyPassword('wrong', hash)).toBe(false);
  });

  it('rejects missing or malformed hashes', () => {
    expect(verifyPassword('q', null)).toBe(false);
    expect(verifyPassword('q', 'not-base64!')).toBe(false);
  });
});

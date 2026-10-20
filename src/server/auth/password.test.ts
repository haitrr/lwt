import { describe, expect, it } from 'vitest';
import { hashPassword, verifyDummyPassword, verifyPassword } from './password';

describe('ASP.NET Identity password hashes', () => {
  // Created by the .NET API (Identity V3, HMAC-SHA256, 10k iterations) for the password "q".
  const dotnetHash = 'AQAAAAEAACcQAAAAEBHYa2WhARafxqD3RGYCyMXqnmZK7h6yyzkTM3YHClO4IDTH4vS84GM8o5RdG9+J4Q==';

  it('verifies hashes made by the .NET API', async () => {
    expect(await verifyPassword('q', dotnetHash)).toBe(true);
    expect(await verifyPassword('Q', dotnetHash)).toBe(false);
  });

  it('round-trips new hashes', async () => {
    const hash = await hashPassword('correct horse');
    expect(Buffer.from(hash, 'base64')[0]).toBe(0x01);
    expect(await verifyPassword('correct horse', hash)).toBe(true);
    expect(await verifyPassword('wrong', hash)).toBe(false);
  });

  it('rejects missing or malformed hashes', async () => {
    expect(await verifyPassword('q', null)).toBe(false);
    expect(await verifyPassword('q', 'not-base64!')).toBe(false);
  });

  it('does not block the event loop while hashing', async () => {
    let ticked = false;
    setImmediate(() => (ticked = true));
    const hashing = hashPassword('correct horse');
    await new Promise((resolve) => setImmediate(resolve));
    expect(ticked).toBe(true);
    await hashing;
  });

  it('never accepts a password for a missing user', async () => {
    expect(await verifyDummyPassword('')).toBe(false);
    expect(await verifyDummyPassword('correct horse')).toBe(false);
  });
});

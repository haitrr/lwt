import { pbkdf2Sync, randomBytes, timingSafeEqual } from 'node:crypto';

// ASP.NET Core Identity PasswordHasher formats, so accounts created by the .NET API keep working.
// V2: 0x00 | salt(16) | subkey(32), PBKDF2-HMAC-SHA1, 1000 iterations.
// V3: 0x01 | prf(u32 BE) | iterations(u32 BE) | saltLength(u32 BE) | salt | subkey.

const PRF_DIGESTS: Record<number, string> = { 0: 'sha1', 1: 'sha256', 2: 'sha512' };

// What Identity in .NET 7+ uses for new hashes; .NET 6 can still verify them.
const NEW_PRF = 2;
const NEW_ITERATIONS = 100_000;
const SALT_LENGTH = 16;
const SUBKEY_LENGTH = 32;

export function hashPassword(password: string): string {
  const salt = randomBytes(SALT_LENGTH);
  const subkey = pbkdf2Sync(password, salt, NEW_ITERATIONS, SUBKEY_LENGTH, PRF_DIGESTS[NEW_PRF]);
  const header = Buffer.alloc(13);
  header.writeUInt8(0x01, 0);
  header.writeUInt32BE(NEW_PRF, 1);
  header.writeUInt32BE(NEW_ITERATIONS, 5);
  header.writeUInt32BE(SALT_LENGTH, 9);
  return Buffer.concat([header, salt, subkey]).toString('base64');
}

export function verifyPassword(password: string, hashed: string | null): boolean {
  if (!hashed) return false;
  const bytes = Buffer.from(hashed, 'base64');
  if (bytes.length === 0) return false;

  if (bytes[0] === 0x00) {
    if (bytes.length !== 1 + 16 + 32) return false;
    const salt = bytes.subarray(1, 17);
    const expected = bytes.subarray(17);
    return timingSafeEqual(pbkdf2Sync(password, salt, 1000, 32, 'sha1'), expected);
  }

  if (bytes[0] === 0x01) {
    if (bytes.length < 13) return false;
    const digest = PRF_DIGESTS[bytes.readUInt32BE(1)];
    const iterations = bytes.readUInt32BE(5);
    const saltLength = bytes.readUInt32BE(9);
    if (!digest || saltLength < 16 || bytes.length < 13 + saltLength + 16) return false;
    const salt = bytes.subarray(13, 13 + saltLength);
    const expected = bytes.subarray(13 + saltLength);
    return timingSafeEqual(pbkdf2Sync(password, salt, iterations, expected.length, digest), expected);
  }

  return false;
}

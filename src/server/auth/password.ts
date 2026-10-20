import { pbkdf2 as pbkdf2Callback, randomBytes, timingSafeEqual } from 'node:crypto';
import { promisify } from 'node:util';

// ASP.NET Core Identity PasswordHasher formats, so accounts created by the .NET API keep working.
// V2: 0x00 | salt(16) | subkey(32), PBKDF2-HMAC-SHA1, 1000 iterations.
// V3: 0x01 | prf(u32 BE) | iterations(u32 BE) | saltLength(u32 BE) | salt | subkey.

const PRF_DIGESTS: Record<number, string> = { 0: 'sha1', 1: 'sha256', 2: 'sha512' };

// What Identity in .NET 7+ uses for new hashes; .NET 6 can still verify them.
const NEW_PRF = 2;
const NEW_ITERATIONS = 100_000;
const SALT_LENGTH = 16;
const SUBKEY_LENGTH = 32;

// Async so a login doesn't hold up every other request for the ~100 ms PBKDF2 takes.
const pbkdf2 = promisify(pbkdf2Callback);

export async function hashPassword(password: string): Promise<string> {
  const salt = randomBytes(SALT_LENGTH);
  const subkey = await pbkdf2(password, salt, NEW_ITERATIONS, SUBKEY_LENGTH, PRF_DIGESTS[NEW_PRF]);
  const header = Buffer.alloc(13);
  header.writeUInt8(0x01, 0);
  header.writeUInt32BE(NEW_PRF, 1);
  header.writeUInt32BE(NEW_ITERATIONS, 5);
  header.writeUInt32BE(SALT_LENGTH, 9);
  return Buffer.concat([header, salt, subkey]).toString('base64');
}

export async function verifyPassword(password: string, hashed: string | null): Promise<boolean> {
  if (!hashed) return false;
  const bytes = Buffer.from(hashed, 'base64');
  if (bytes.length === 0) return false;

  if (bytes[0] === 0x00) {
    if (bytes.length !== 1 + 16 + 32) return false;
    const salt = bytes.subarray(1, 17);
    const expected = bytes.subarray(17);
    return timingSafeEqual(await pbkdf2(password, salt, 1000, 32, 'sha1'), expected);
  }

  if (bytes[0] === 0x01) {
    if (bytes.length < 13) return false;
    const digest = PRF_DIGESTS[bytes.readUInt32BE(1)];
    const iterations = bytes.readUInt32BE(5);
    const saltLength = bytes.readUInt32BE(9);
    if (!digest || saltLength < 16 || bytes.length < 13 + saltLength + 16) return false;
    const salt = bytes.subarray(13, 13 + saltLength);
    const expected = bytes.subarray(13 + saltLength);
    return timingSafeEqual(await pbkdf2(password, salt, iterations, expected.length, digest), expected);
  }

  return false;
}

let dummyHash: Promise<string> | undefined;

/**
 * Does the same work as checking a password against a new hash, for when the user doesn't exist,
 * so the response time doesn't tell which user names are taken.
 */
export async function verifyDummyPassword(password: string): Promise<false> {
  await verifyPassword(password, await (dummyHash ??= hashPassword(randomBytes(16).toString('hex'))));
  return false;
}

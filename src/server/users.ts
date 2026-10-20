import 'server-only';
import { eq, sql } from 'drizzle-orm';
import { getDb, schema, type Tx } from './db';
import { UserError } from './errors';
import { hashPassword, verifyDummyPassword, verifyPassword } from './auth/password';
import { isLanguageCode } from '@/lib/languages';

const { users, userSettings, languageSettings } = schema;

export type LanguageSettingView = { id: number; languageCode: string; dictionaryLanguageCode: string };
/** What the profile page shows of the Microsoft Translator settings; the key itself never leaves the server. */
export type TranslatorSettingsView = { hasKey: boolean; region: string };

/** Only checked when a password is set, so older accounts with shorter ones can still sign in. */
export const MIN_PASSWORD_LENGTH = 8;

function checkNewPassword(password: string) {
  if (!password) throw new UserError('Password is required.');
  if (password.length < MIN_PASSWORD_LENGTH) {
    throw new UserError(`Password must be at least ${MIN_PASSWORD_LENGTH} characters.`);
  }
}

/** Postgres' unique_violation, which drizzle passes on as the cause of its own error. */
function isUniqueViolation(e: unknown) {
  const err = (e as { code?: unknown; cause?: { code?: unknown } } | null);
  return err?.code === '23505' || err?.cause?.code === '23505';
}

export async function signUp(userName: string, password: string) {
  userName = userName.trim();
  if (!userName) throw new UserError('User name is required.');
  checkNewPassword(password);
  const passwordHash = await hashPassword(password);
  try {
    await getDb().transaction(async (tx) => {
      const [{ id }] = await tx
        .insert(users)
        .values({ userName, normalizedUserName: userName.toUpperCase(), passwordHash })
        .returning({ id: users.id });
      await tx.insert(userSettings).values({ userId: id });
    });
  } catch (e) {
    if (isUniqueViolation(e)) throw new UserError(`Username '${userName}' is already taken.`);
    throw e;
  }
}

/** Returns the user when the credentials are right. */
export async function checkCredentials(userName: string, password: string) {
  const [user] = await getDb()
    .select({ id: users.id, userName: users.userName, passwordHash: users.passwordHash, tokenVersion: users.tokenVersion })
    .from(users)
    .where(eq(users.normalizedUserName, userName.trim().toUpperCase()));
  // Hash the password either way, so a missing user takes as long as a wrong password.
  const valid = user ? await verifyPassword(password, user.passwordHash) : await verifyDummyPassword(password);
  if (!user || !valid) throw new UserError('Username or password is incorrect.');
  return { id: user.id, userName: user.userName ?? userName, tokenVersion: user.tokenVersion };
}

/** Changes the password and signs out every session; returns the new token version for the current one. */
export async function changePassword(userId: number, currentPassword: string, newPassword: string) {
  const db = getDb();
  const [user] = await db.select({ passwordHash: users.passwordHash }).from(users).where(eq(users.id, userId));
  if (!user) throw new UserError('User not found.', 404);
  if (!(await verifyPassword(currentPassword, user.passwordHash))) {
    throw new UserError('Current password is not correct.');
  }
  checkNewPassword(newPassword);
  const [updated] = await db
    .update(users)
    .set({ passwordHash: await hashPassword(newPassword), tokenVersion: sql`${users.tokenVersion} + 1` })
    .where(eq(users.id, userId))
    .returning({ userName: users.userName, tokenVersion: users.tokenVersion });
  return updated;
}

export async function getUser(userId: number) {
  const [user] = await getDb()
    .select({ userName: users.userName, email: users.email })
    .from(users)
    .where(eq(users.id, userId));
  if (!user) throw new UserError('User not found.', 404);
  return user;
}

export async function getLanguageSettings(userId: number): Promise<LanguageSettingView[]> {
  return getDb()
    .select({
      id: languageSettings.id,
      languageCode: languageSettings.languageCode,
      dictionaryLanguageCode: languageSettings.dictionaryLanguageCode,
    })
    .from(languageSettings)
    .innerJoin(userSettings, eq(languageSettings.userSettingId, userSettings.id))
    .where(eq(userSettings.userId, userId));
}

export async function saveLanguageSettings(
  userId: number,
  settings: { languageCode: string; dictionaryLanguageCode: string }[],
) {
  for (const s of settings) {
    if (!isLanguageCode(s.languageCode) || !isLanguageCode(s.dictionaryLanguageCode)) {
      throw new UserError('Unsupported language.');
    }
  }
  await getDb().transaction(async (tx) => {
    const setting = await ensureUserSetting(tx, userId);
    await tx.delete(languageSettings).where(eq(languageSettings.userSettingId, setting.id));
    if (settings.length) {
      await tx.insert(languageSettings).values(settings.map((s) => ({ ...s, userSettingId: setting.id })));
    }
  });
}

/** The user's settings row; users imported from the old database may not have one yet. */
async function ensureUserSetting(tx: Tx, userId: number) {
  const [setting] = await tx.select({ id: userSettings.id }).from(userSettings).where(eq(userSettings.userId, userId));
  if (setting) return setting;
  const [{ id }] = await tx.insert(userSettings).values({ userId }).returning({ id: userSettings.id });
  return { id };
}

/** The user's Microsoft Translator key and region, or null when no key is set. */
export async function getTranslatorSettings(userId: number) {
  const [setting] = await getDb()
    .select({ key: userSettings.azureTranslatorKey, region: userSettings.azureTranslatorRegion })
    .from(userSettings)
    .where(eq(userSettings.userId, userId));
  return setting?.key ? { key: setting.key, region: setting.region || null } : null;
}

export async function getTranslatorSettingsView(userId: number): Promise<TranslatorSettingsView> {
  const settings = await getTranslatorSettings(userId);
  return { hasKey: settings != null, region: settings?.region ?? '' };
}

/** A blank key keeps the saved one; null removes it. */
export async function saveTranslatorSettings(userId: number, key: string | null, region: string) {
  key = key?.trim() ?? null;
  await getDb().transaction(async (tx) => {
    const setting = await ensureUserSetting(tx, userId);
    await tx
      .update(userSettings)
      .set({
        ...(key === '' ? {} : { azureTranslatorKey: key }),
        azureTranslatorRegion: key === null ? null : region.trim() || null,
      })
      .where(eq(userSettings.id, setting.id));
  });
}

import 'server-only';
import { eq } from 'drizzle-orm';
import { getDb, schema } from './db';
import { UserError } from './errors';
import { hashPassword, verifyPassword } from './auth/password';
import { isLanguageCode } from '@/lib/languages';

const { users, userSettings, languageSettings } = schema;

export type LanguageSettingView = { id: number; languageCode: string; dictionaryLanguageCode: string };

export async function signUp(userName: string, password: string) {
  userName = userName.trim();
  if (!userName) throw new UserError('User name is required.');
  if (!password) throw new UserError('Password is required.');
  const db = getDb();
  const normalized = userName.toUpperCase();
  const [existing] = await db.select({ id: users.id }).from(users).where(eq(users.normalizedUserName, normalized));
  if (existing) throw new UserError(`Username '${userName}' is already taken.`);

  await db.transaction(async (tx) => {
    const [{ id }] = await tx
      .insert(users)
      .values({
        userName,
        normalizedUserName: normalized,
        passwordHash: hashPassword(password),
      })
      .returning({ id: users.id });
    await tx.insert(userSettings).values({ userId: id });
  });
}

/** Returns the user id when the credentials are right. */
export async function checkCredentials(userName: string, password: string) {
  const [user] = await getDb()
    .select({ id: users.id, userName: users.userName, passwordHash: users.passwordHash })
    .from(users)
    .where(eq(users.normalizedUserName, userName.trim().toUpperCase()));
  if (!user || !verifyPassword(password, user.passwordHash)) {
    throw new UserError('Username or password is incorrect.');
  }
  return { id: user.id, userName: user.userName ?? userName };
}

export async function changePassword(userId: number, currentPassword: string, newPassword: string) {
  const db = getDb();
  const [user] = await db.select({ passwordHash: users.passwordHash }).from(users).where(eq(users.id, userId));
  if (!user) throw new UserError('User not found.', 404);
  if (!verifyPassword(currentPassword, user.passwordHash)) throw new UserError('Current password is not correct.');
  if (!newPassword) throw new UserError('Password is required.');
  await db
    .update(users)
    .set({ passwordHash: hashPassword(newPassword) })
    .where(eq(users.id, userId));
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
    let [setting] = await tx.select({ id: userSettings.id }).from(userSettings).where(eq(userSettings.userId, userId));
    if (!setting) {
      const [{ id }] = await tx.insert(userSettings).values({ userId }).returning({ id: userSettings.id });
      setting = { id };
    }
    await tx.delete(languageSettings).where(eq(languageSettings.userSettingId, setting.id));
    if (settings.length) {
      await tx.insert(languageSettings).values(settings.map((s) => ({ ...s, userSettingId: setting.id })));
    }
  });
}

import { bigint, boolean, datetime, int, longtext, mysqlTable, varchar } from 'drizzle-orm/mysql-core';

// Mirrors the tables the .NET API (EF Core + ASP.NET Identity) created, so the same database
// keeps working. Column names are PascalCase because that is what EF generated.

const timestamps = {
  createdDate: datetime('CreatedDate', { fsp: 6 })
    .notNull()
    .$defaultFn(() => new Date()),
  lastModifiedDate: datetime('LastModifiedDate', { fsp: 6 })
    .notNull()
    .$defaultFn(() => new Date())
    .$onUpdateFn(() => new Date()),
};

export const users = mysqlTable('users', {
  id: int('Id').primaryKey().autoincrement(),
  userName: varchar('UserName', { length: 256 }),
  normalizedUserName: varchar('NormalizedUserName', { length: 256 }),
  email: varchar('Email', { length: 256 }),
  normalizedEmail: varchar('NormalizedEmail', { length: 256 }),
  emailConfirmed: boolean('EmailConfirmed').notNull().$defaultFn(() => false),
  passwordHash: longtext('PasswordHash'),
  securityStamp: longtext('SecurityStamp'),
  concurrencyStamp: longtext('ConcurrencyStamp'),
  phoneNumber: longtext('PhoneNumber'),
  phoneNumberConfirmed: boolean('PhoneNumberConfirmed').notNull().$defaultFn(() => false),
  twoFactorEnabled: boolean('TwoFactorEnabled').notNull().$defaultFn(() => false),
  lockoutEnd: datetime('LockoutEnd', { fsp: 6 }),
  lockoutEnabled: boolean('LockoutEnabled').notNull().$defaultFn(() => true),
  accessFailedCount: int('AccessFailedCount').notNull().$defaultFn(() => 0),
  currentLanguageId: int('CurrentLanguageId'),
});

export const texts = mysqlTable('texts', {
  id: int('Id').primaryKey().autoincrement(),
  ...timestamps,
  userId: int('UserId').notNull(),
  bookmark: bigint('Bookmark', { mode: 'number', unsigned: true }),
  title: longtext('Title').notNull(),
  content: longtext('Content').notNull(),
  languageCode: longtext('LanguageCode').notNull(),
  termCount: bigint('TermCount', { mode: 'number' }).notNull().$defaultFn(() => 0),
  processedTermCount: bigint('ProcessedTermCount', { mode: 'number' }).notNull().$defaultFn(() => 0),
  lastReadAt: datetime('LastReadAt', { fsp: 6 })
    .notNull()
    .$defaultFn(() => new Date()),
});

export const terms = mysqlTable('terms', {
  id: int('Id').primaryKey().autoincrement(),
  ...timestamps,
  // utf8mb4_0900_ai_ci: lookups are case- and accent-insensitive, and the unique index
  // (Content, LanguageCode, UserId) treats e.g. ｱ and ア as the same term.
  content: varchar('Content', { length: 95 }).notNull(),
  languageCode: varchar('LanguageCode', { length: 95 }).notNull(),
  meaning: longtext('Meaning').notNull(),
  userId: int('UserId').notNull(),
  learningLevel: longtext('LearningLevel').notNull(),
});

export const textTerms = mysqlTable('text_terms', {
  id: int('Id').primaryKey().autoincrement(),
  ...timestamps,
  textId: int('TextId').notNull(),
  termId: int('TermId'),
  content: longtext('Content').notNull(),
  index: bigint('Index', { mode: 'number' }).notNull(),
});

export const userSettings = mysqlTable('user_settings', {
  id: int('Id').primaryKey().autoincrement(),
  ...timestamps,
  userId: int('UserId').notNull(),
});

export const languageSettings = mysqlTable('language_settings', {
  id: int('Id').primaryKey().autoincrement(),
  ...timestamps,
  dictionaryLanguageCode: longtext('DictionaryLanguageCode').notNull(),
  languageCode: longtext('LanguageCode').notNull(),
  userSettingId: int('UserSettingId'),
});

export const logs = mysqlTable('logs', {
  id: int('Id').primaryKey().autoincrement(),
  ...timestamps,
  event: longtext('Event').notNull(),
  data: longtext('Data').notNull(),
  userName: longtext('UserName').notNull(),
  entityId: int('EntityId'),
  entityType: longtext('EntityType'),
});

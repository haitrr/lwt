import { bigint, customType, index, integer, pgTable, primaryKey, serial, text, timestamp, uniqueIndex, varchar } from 'drizzle-orm/pg-core';

// Tables and columns follow the old .NET API's MySQL schema, renamed to snake_case.
// scripts/import-mysql.mjs copies an existing database over.

const timestamps = {
  createdDate: timestamp('created_date', { withTimezone: true, precision: 6 })
    .notNull()
    .$defaultFn(() => new Date()),
  lastModifiedDate: timestamp('last_modified_date', { withTimezone: true, precision: 6 })
    .notNull()
    .$defaultFn(() => new Date())
    .$onUpdateFn(() => new Date()),
};

/**
 * Term content compares like MySQL's utf8mb4_0900_ai_ci did: ignoring case, accents and width,
 * so e.g. ｱ and ア are the same term. The lwt_ci collation is created by the first migration.
 */
const termContent = customType<{ data: string }>({
  dataType: () => 'varchar(95) COLLATE lwt_ci',
});

export const users = pgTable(
  'users',
  {
    id: serial('id').primaryKey(),
    userName: varchar('user_name', { length: 256 }).notNull(),
    normalizedUserName: varchar('normalized_user_name', { length: 256 }).notNull(),
    email: varchar('email', { length: 256 }),
    // ASP.NET Identity V2/V3 hashes, so passwords from the old API keep working.
    passwordHash: text('password_hash').notNull(),
  },
  (t) => [uniqueIndex('users_normalized_user_name_key').on(t.normalizedUserName)],
);

export const texts = pgTable(
  'texts',
  {
    id: serial('id').primaryKey(),
    ...timestamps,
    userId: integer('user_id')
      .notNull()
      .references(() => users.id, { onDelete: 'cascade' }),
    bookmark: bigint('bookmark', { mode: 'number' }),
    title: text('title').notNull(),
    content: text('content').notNull(),
    languageCode: text('language_code').notNull(),
    termCount: bigint('term_count', { mode: 'number' })
      .notNull()
      .$defaultFn(() => 0),
    processedTermCount: bigint('processed_term_count', { mode: 'number' })
      .notNull()
      .$defaultFn(() => 0),
    lastReadAt: timestamp('last_read_at', { withTimezone: true, precision: 6 })
      .notNull()
      .$defaultFn(() => new Date()),
  },
  (t) => [index('texts_user_id_idx').on(t.userId), index('texts_processed_term_count_idx').on(t.processedTermCount)],
);

export const terms = pgTable(
  'terms',
  {
    id: serial('id').primaryKey(),
    ...timestamps,
    content: termContent('content').notNull(),
    languageCode: varchar('language_code', { length: 95 }).notNull(),
    meaning: text('meaning').notNull(),
    userId: integer('user_id')
      .notNull()
      .references(() => users.id, { onDelete: 'cascade' }),
    learningLevel: text('learning_level').notNull(),
  },
  (t) => [
    uniqueIndex('terms_content_language_code_user_id_key').on(t.content, t.languageCode, t.userId),
    index('terms_user_id_idx').on(t.userId),
  ],
);

export const textTerms = pgTable(
  'text_terms',
  {
    id: serial('id').primaryKey(),
    ...timestamps,
    textId: integer('text_id')
      .notNull()
      .references(() => texts.id, { onDelete: 'cascade' }),
    termId: integer('term_id').references(() => terms.id),
    content: text('content').notNull(),
    index: bigint('index', { mode: 'number' }).notNull(),
  },
  (t) => [
    index('text_terms_text_id_index_idx').on(t.textId, t.index),
    index('text_terms_text_id_term_id_idx').on(t.textId, t.termId),
    index('text_terms_term_id_idx').on(t.termId),
  ],
);

export const userSettings = pgTable(
  'user_settings',
  {
    id: serial('id').primaryKey(),
    ...timestamps,
    userId: integer('user_id')
      .notNull()
      .references(() => users.id, { onDelete: 'cascade' }),
    // Microsoft Translator, asked when the dictionary service has no entry.
    azureTranslatorKey: text('azure_translator_key'),
    azureTranslatorRegion: text('azure_translator_region'),
  },
  (t) => [uniqueIndex('user_settings_user_id_key').on(t.userId)],
);

export const languageSettings = pgTable(
  'language_settings',
  {
    id: serial('id').primaryKey(),
    ...timestamps,
    dictionaryLanguageCode: text('dictionary_language_code').notNull(),
    languageCode: text('language_code').notNull(),
    userSettingId: integer('user_setting_id').references(() => userSettings.id),
  },
  (t) => [index('language_settings_user_setting_id_idx').on(t.userSettingId)],
);

export const logs = pgTable(
  'logs',
  {
    id: serial('id').primaryKey(),
    ...timestamps,
    event: text('event').notNull(),
    data: text('data').notNull(),
    userName: text('user_name').notNull(),
    entityId: integer('entity_id'),
    entityType: text('entity_type'),
  },
  // For the statistics page's activity, read per user over the last year.
  (t) => [index('logs_user_name_created_date_idx').on(t.userName, t.createdDate)],
);

// Built-in dictionaries (dictionary/*.tsv.gz, made by scripts/build-dictionary.mjs), loaded at startup by
// src/server/dictionaryImport.ts. One row per headword and dictionary pair; meaning lines are separated by \n.
export const dictionaries = pgTable(
  'dictionaries',
  {
    languageCode: text('language_code').notNull(),
    dictionaryLanguageCode: text('dictionary_language_code').notNull(),
    source: text('source').notNull(),
    // Checksum of the imported file; a different one at startup means re-import.
    sha256: text('sha256').notNull(),
    importedAt: timestamp('imported_at', { withTimezone: true, precision: 6 }).notNull(),
  },
  (t) => [primaryKey({ columns: [t.languageCode, t.dictionaryLanguageCode] })],
);

export const dictionaryEntries = pgTable(
  'dictionary_entries',
  {
    languageCode: text('language_code').notNull(),
    dictionaryLanguageCode: text('dictionary_language_code').notNull(),
    word: text('word').notNull(),
    meaning: text('meaning').notNull(),
  },
  (t) => [primaryKey({ columns: [t.languageCode, t.dictionaryLanguageCode, t.word] })],
);

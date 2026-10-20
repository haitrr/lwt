// Copies the data of the old .NET API's MySQL database into the Postgres database.
//
//   MYSQL_URL=mysql://user:pass@host:3306/lwt DATABASE_URL=postgres://user:pass@host:5432/lwt npm run import-mysql
//
// It creates the schema first and refuses to run when the Postgres database already has users.
// Ids are kept, so links to texts stay valid. MySQL is only read.

import mysql from 'mysql2/promise';
import pg from 'pg';
import { drizzle } from 'drizzle-orm/node-postgres';
import { migrate } from 'drizzle-orm/node-postgres/migrator';

const BATCH = 5000;

const mysqlUrl = process.env.MYSQL_URL;
const pgUrl = process.env.DATABASE_URL;
if (!mysqlUrl || !pgUrl) {
  console.error('Set MYSQL_URL (the old database) and DATABASE_URL (the new Postgres database).');
  process.exit(1);
}

// EF stored DateTime as UTC without a zone.
const source = await mysql.createConnection({ uri: mysqlUrl, timezone: 'Z', supportBigNumbers: true });
const pool = new pg.Pool({ connectionString: pgUrl });

await migrate(drizzle(pool), { migrationsFolder: new URL('../drizzle', import.meta.url).pathname });

const target = await pool.connect();
const {
  rows: [{ n: existingUsers }],
} = await target.query('select count(*)::int as n from users');
if (existingUsers > 0) {
  console.error('The Postgres database already has users; import into an empty database.');
  process.exit(1);
}

/** Inserts rows (arrays in column order) with one multi-row statement. */
async function insert(table, columns, rows, suffix = '') {
  if (!rows.length) return { rows: [] };
  const params = [];
  const values = rows.map((row) => `(${row.map((v) => (params.push(v), `$${params.length}`)).join(', ')})`);
  const cols = columns.map((c) => `"${c}"`).join(', ');
  return target.query(`insert into "${table}" (${cols}) values ${values.join(', ')} ${suffix}`, params);
}

/** Reads a MySQL table in Id order, BATCH rows at a time. */
async function* batches(table, columns, size = BATCH) {
  let lastId = 0;
  for (;;) {
    const [rows] = await source.query(
      `select ${columns.map((c) => `\`${c}\``).join(', ')} from \`${table}\` where Id > ? order by Id limit ?`,
      [lastId, size],
    );
    if (!rows.length) return;
    yield rows;
    lastId = rows.at(-1).Id;
  }
}

const counts = {};
async function copy(table, sourceColumns, targetColumns, map = (r) => sourceColumns.map((c) => r[c]), size = BATCH) {
  counts[table] = 0;
  for await (const rows of batches(table, sourceColumns, size)) {
    await insert(table, targetColumns, rows.map(map));
    counts[table] += rows.length;
  }
  console.log(`${table}: ${counts[table]}`);
}

const stamps = ['CreatedDate', 'LastModifiedDate'];
const stampColumns = ['created_date', 'last_modified_date'];

await target.query('begin');

await copy(
  'users',
  ['Id', 'UserName', 'NormalizedUserName', 'Email', 'PasswordHash'],
  ['id', 'user_name', 'normalized_user_name', 'email', 'password_hash'],
  (r) => [r.Id, r.UserName ?? '', r.NormalizedUserName ?? (r.UserName ?? '').toUpperCase(), r.Email, r.PasswordHash ?? ''],
);
await copy('user_settings', ['Id', ...stamps, 'UserId'], ['id', ...stampColumns, 'user_id']);
await copy(
  'language_settings',
  ['Id', ...stamps, 'DictionaryLanguageCode', 'LanguageCode', 'UserSettingId'],
  ['id', ...stampColumns, 'dictionary_language_code', 'language_code', 'user_setting_id'],
);
await copy(
  'texts',
  ['Id', ...stamps, 'UserId', 'Bookmark', 'Title', 'Content', 'LanguageCode', 'TermCount', 'ProcessedTermCount', 'LastReadAt'],
  ['id', ...stampColumns, 'user_id', 'bookmark', 'title', 'content', 'language_code', 'term_count', 'processed_term_count', 'last_read_at'],
  undefined,
  // A text can be a whole book.
  50,
);

// Postgres' term comparison is a close match to MySQL's, but not exact (it also treats ' and ’
// as equal). Terms that become duplicates are merged into the oldest one.
const termColumns = ['Id', ...stamps, 'Content', 'LanguageCode', 'Meaning', 'UserId', 'LearningLevel'];
const mergedInto = new Map();
counts.terms = 0;
for await (const rows of batches('terms', termColumns)) {
  const { rows: inserted } = await insert(
    'terms',
    ['id', ...stampColumns, 'content', 'language_code', 'meaning', 'user_id', 'learning_level'],
    rows.map((r) => termColumns.map((c) => r[c])),
    'on conflict do nothing returning id',
  );
  const ids = new Set(inserted.map((r) => r.id));
  for (const r of rows.filter((r) => !ids.has(r.Id))) {
    const {
      rows: [kept],
    } = await target.query(
      'select id, content from terms where content = $1 and language_code = $2 and user_id = $3',
      [r.Content, r.LanguageCode, r.UserId],
    );
    mergedInto.set(r.Id, kept.id);
    console.log(`Merged term ${r.Id} "${r.Content}" into ${kept.id} "${kept.content}"`);
  }
  counts.terms += rows.length;
}
console.log(`terms: ${counts.terms}${mergedInto.size ? ` (${mergedInto.size} merged)` : ''}`);

await copy(
  'text_terms',
  ['Id', ...stamps, 'TextId', 'TermId', 'Content', 'Index'],
  ['id', ...stampColumns, 'text_id', 'term_id', 'content', 'index'],
  (r) => [r.Id, r.CreatedDate, r.LastModifiedDate, r.TextId, mergedInto.get(r.TermId) ?? r.TermId, r.Content, r.Index],
);
await copy(
  'logs',
  ['Id', ...stamps, 'Event', 'Data', 'UserName', 'EntityId', 'EntityType'],
  ['id', ...stampColumns, 'event', 'data', 'user_name', 'entity_id', 'entity_type'],
);

// New rows continue after the copied ids.
for (const table of Object.keys(counts)) {
  await target.query(`select setval(pg_get_serial_sequence('${table}', 'id'), coalesce((select max(id) from "${table}"), 0) + 1, false)`);
}

await target.query('commit');
target.release();
await pool.end();
await source.end();
console.log('Done.');

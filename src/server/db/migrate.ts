import path from 'node:path';
import { migrate } from 'drizzle-orm/node-postgres/migrator';
import { getDb } from '.';

/** Brings the database schema up to date; run once at startup. */
export async function migrateDatabase() {
  await migrate(getDb(), { migrationsFolder: path.join(process.cwd(), 'drizzle') });
}

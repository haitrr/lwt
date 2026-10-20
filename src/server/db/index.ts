import 'server-only';
import { drizzle } from 'drizzle-orm/mysql2';
import mysql from 'mysql2/promise';
import * as schema from './schema';

declare global {
  var lwtPool: mysql.Pool | undefined;
}

function createPool() {
  const url = process.env.DATABASE_URL;
  if (!url) throw new Error('DATABASE_URL is not set');
  // EF stored DateTime as UTC without a zone; read and write it the same way.
  return mysql.createPool({ uri: url, timezone: 'Z', connectionLimit: 10 });
}

function createDb() {
  // Reuse one pool across dev hot reloads.
  const pool = (globalThis.lwtPool ??= createPool());
  return drizzle(pool, { schema, mode: 'default' });
}

export type Db = ReturnType<typeof createDb>;

let instance: Db | undefined;

/** Created on first use so builds don't need DATABASE_URL. */
export function getDb(): Db {
  return (instance ??= createDb());
}

export type Tx = Parameters<Parameters<Db['transaction']>[0]>[0];
export { schema };

import 'server-only';
import { drizzle } from 'drizzle-orm/node-postgres';
import { Pool } from 'pg';
import * as schema from './schema';

declare global {
  var lwtPool: Pool | undefined;
}

function createPool() {
  const connectionString = process.env.DATABASE_URL;
  if (!connectionString) throw new Error('DATABASE_URL is not set');
  const pool = new Pool({ connectionString, max: 10 });
  // An idle client losing its connection (e.g. Postgres restarting) emits 'error' on the pool, which
  // crashes the process when unhandled. The pool drops that client; the next query opens a new one.
  pool.on('error', (e) => console.error(`Idle Postgres client failed: ${e.message}`));
  return pool;
}

function createDb() {
  // Reuse one pool across dev hot reloads.
  const pool = (globalThis.lwtPool ??= createPool());
  return drizzle(pool, { schema });
}

export type Db = ReturnType<typeof createDb>;

let instance: Db | undefined;

/** Created on first use so builds don't need DATABASE_URL. */
export function getDb(): Db {
  return (instance ??= createDb());
}

export type Tx = Parameters<Parameters<Db['transaction']>[0]>[0];
export { schema };

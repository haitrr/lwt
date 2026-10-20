const MAX_ID = 2 ** 31 - 1;

/** A route segment as a database id (a positive Postgres integer), or null when it isn't one. */
export function parseId(segment: string): number | null {
  if (!/^[1-9]\d{0,9}$/.test(segment)) return null;
  const id = Number(segment);
  return id <= MAX_ID ? id : null;
}

/**
 * Counts attempts per key in fixed windows, in memory: fine for the single Node instance this runs as.
 * Keeps at most `maxKeys` keys, dropping the least recently used, so a flood of keys can't exhaust memory.
 */
export class AttemptLimiter {
  private readonly entries = new Map<string, { count: number; resetAt: number }>();

  constructor(
    private readonly options: { max: number; windowMs: number; maxKeys?: number },
    private readonly now: () => number = Date.now,
  ) {}

  /** Records an attempt; returns the milliseconds to wait when `key` is over its limit, else 0. */
  consume(key: string): number {
    const now = this.now();
    let entry = this.entries.get(key);
    if (!entry || entry.resetAt <= now) entry = { count: 0, resetAt: now + this.options.windowMs };
    this.entries.delete(key);
    this.entries.set(key, entry);
    if (this.entries.size > (this.options.maxKeys ?? 50_000)) {
      this.entries.delete(this.entries.keys().next().value!);
    }
    if (entry.count >= this.options.max) return entry.resetAt - now;
    entry.count++;
    return 0;
  }

  reset(key: string) {
    this.entries.delete(key);
  }
}

import { describe, expect, it } from 'vitest';
import { AttemptLimiter } from './rateLimit';

describe('AttemptLimiter', () => {
  it('allows max attempts per window, then reports the wait', () => {
    let now = 0;
    const limiter = new AttemptLimiter({ max: 2, windowMs: 1000 }, () => now);
    expect(limiter.consume('a')).toBe(0);
    expect(limiter.consume('a')).toBe(0);
    now = 400;
    expect(limiter.consume('a')).toBe(600);
    expect(limiter.consume('b')).toBe(0);
    now = 1000;
    expect(limiter.consume('a')).toBe(0);
  });

  it('starts over after a reset', () => {
    const limiter = new AttemptLimiter({ max: 1, windowMs: 1000 }, () => 0);
    limiter.consume('a');
    expect(limiter.consume('a')).toBeGreaterThan(0);
    limiter.reset('a');
    expect(limiter.consume('a')).toBe(0);
  });

  it('forgets the least recently used key when full', () => {
    const limiter = new AttemptLimiter({ max: 1, windowMs: 1000, maxKeys: 2 }, () => 0);
    limiter.consume('a');
    limiter.consume('b');
    limiter.consume('a'); // a is now the most recently used
    limiter.consume('c'); // drops b
    expect(limiter.consume('a')).toBeGreaterThan(0);
    expect(limiter.consume('b')).toBe(0);
  });
});

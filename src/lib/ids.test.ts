import { describe, expect, it } from 'vitest';
import { parseId } from './ids';

describe('parseId', () => {
  it('accepts positive 32-bit integers', () => {
    expect(parseId('1')).toBe(1);
    expect(parseId('42')).toBe(42);
    expect(parseId('2147483647')).toBe(2147483647);
  });

  it.each(['', '0', '-1', '01', '1.5', '1e3', ' 1', '0x10', 'abc', '2147483648', '99999999999999999999'])(
    'rejects %j',
    (segment) => {
      expect(parseId(segment)).toBeNull();
    },
  );
});

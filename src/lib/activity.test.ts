import { describe, expect, it } from 'vitest';
import { addDays, buildCalendar, dateIn, intensity, streaks, utcOffset, validTimeZone } from './activity';

const day = (date: string, marked = 1, read = 0) => ({ date, marked, read });

describe('dateIn', () => {
  it('uses the given time zone', () => {
    const at = new Date('2026-10-19T20:00:00Z');
    expect(dateIn('UTC', at)).toBe('2026-10-19');
    expect(dateIn('Asia/Ho_Chi_Minh', at)).toBe('2026-10-20');
  });
});

describe('validTimeZone', () => {
  it('falls back to UTC', () => {
    expect(validTimeZone(undefined)).toBe('UTC');
    expect(validTimeZone('Not/AZone')).toBe('UTC');
    expect(validTimeZone('Asia/Tokyo')).toBe('Asia/Tokyo');
  });
});

describe('utcOffset', () => {
  it('formats the offset at the given moment', () => {
    const at = new Date('2026-07-01T00:00:00Z');
    expect(utcOffset('UTC', at)).toBe('+00:00');
    expect(utcOffset('Asia/Saigon', at)).toBe('+07:00');
    expect(utcOffset('America/St_Johns', at)).toBe('-02:30');
  });
});

describe('addDays', () => {
  it('crosses month and year ends', () => {
    expect(addDays('2026-12-31', 1)).toBe('2027-01-01');
    expect(addDays('2026-03-01', -1)).toBe('2026-02-28');
  });
});

describe('buildCalendar', () => {
  // 2026-10-21 is a Wednesday.
  const weeks = buildCalendar([day('2026-10-19', 3), day('2026-10-12', 0, 2)], '2026-10-21', 2);

  it('starts weeks on Monday and ends with the current week', () => {
    expect(weeks).toHaveLength(2);
    expect(weeks[0][0].date).toBe('2026-10-12');
    expect(weeks[1][6].date).toBe('2026-10-25');
  });

  it('fills in activity and marks days after today', () => {
    expect(weeks[0][0]).toMatchObject({ read: 2, marked: 0, future: false });
    expect(weeks[1][0]).toMatchObject({ marked: 3, future: false });
    expect(weeks[1][2].future).toBe(false);
    expect(weeks[1][3].future).toBe(true);
  });
});

describe('streaks', () => {
  const days = [day('2026-10-01'), day('2026-10-02'), day('2026-10-03'), day('2026-10-18'), day('2026-10-19')];

  it('counts back from yesterday when today is still empty', () => {
    expect(streaks(days, '2026-10-20')).toEqual({ current: 2, longest: 3 });
  });

  it('includes today', () => {
    expect(streaks([...days, day('2026-10-20')], '2026-10-20').current).toBe(3);
  });

  it('breaks after a missed day', () => {
    expect(streaks(days, '2026-10-21').current).toBe(0);
  });

  it('ignores days without activity', () => {
    expect(streaks([day('2026-10-20', 0, 0)], '2026-10-20')).toEqual({ current: 0, longest: 0 });
  });
});

describe('intensity', () => {
  it('buckets by share of the busiest day', () => {
    expect(intensity(day('x', 0), 40)).toBe(0);
    expect(intensity(day('x', 1), 40)).toBe(1);
    expect(intensity(day('x', 20), 40)).toBe(2);
    expect(intensity(day('x', 40), 40)).toBe(4);
  });
});

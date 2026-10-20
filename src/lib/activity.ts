// Daily activity for the statistics page. Days are YYYY-MM-DD strings in the user's time zone.

/** Cookie the root layout's script sets to the browser's time zone, so days split at the user's midnight. */
export const TIME_ZONE_COOKIE = 'lwt_tz';

export const timeZoneScript = `(function(){try{document.cookie='${TIME_ZONE_COOKIE}='+encodeURIComponent(Intl.DateTimeFormat().resolvedOptions().timeZone)+';path=/;max-age=31536000;samesite=lax'}catch(e){}})()`;

export type ActivityDay = {
  date: string;
  /** Terms created or edited (a new meaning or learning level). */
  marked: number;
  /** Different texts opened. */
  read: number;
};

export type CalendarDay = ActivityDay & { future: boolean };

export function validTimeZone(tz: string | undefined): string {
  if (!tz) return 'UTC';
  try {
    new Intl.DateTimeFormat('en', { timeZone: tz });
    return tz;
  } catch {
    return 'UTC';
  }
}

export function dateIn(timeZone: string, at = new Date()) {
  // en-CA formats as YYYY-MM-DD.
  return new Intl.DateTimeFormat('en-CA', { timeZone, year: 'numeric', month: '2-digit', day: '2-digit' }).format(at);
}

/** The zone's current offset from UTC, like +07:00 or -03:30. */
export function utcOffset(timeZone: string, at = new Date()) {
  const name = new Intl.DateTimeFormat('en', { timeZone, timeZoneName: 'longOffset' })
    .formatToParts(at)
    .find((p) => p.type === 'timeZoneName')!.value;
  return name === 'GMT' ? '+00:00' : name.slice(3);
}

export function addDays(date: string, days: number) {
  const d = new Date(`${date}T00:00:00Z`);
  d.setUTCDate(d.getUTCDate() + days);
  return d.toISOString().slice(0, 10);
}

/** Weeks (Monday first) up to and including today's, oldest first; days after today are marked future. */
export function buildCalendar(days: ActivityDay[], today: string, weekCount: number): CalendarDay[][] {
  const byDate = new Map(days.map((d) => [d.date, d]));
  const weekday = (new Date(`${today}T00:00:00Z`).getUTCDay() + 6) % 7;
  const start = addDays(today, -weekday - (weekCount - 1) * 7);
  return Array.from({ length: weekCount }, (_, w) =>
    Array.from({ length: 7 }, (_, d) => {
      const date = addDays(start, w * 7 + d);
      const day = byDate.get(date);
      return { date, marked: day?.marked ?? 0, read: day?.read ?? 0, future: date > today };
    }),
  );
}

/**
 * Consecutive active days. The current streak still counts when today has no activity yet,
 * as long as yesterday had some.
 */
export function streaks(days: ActivityDay[], today: string) {
  const active = new Set(days.filter((d) => d.marked || d.read).map((d) => d.date));
  let current = 0;
  for (let date = active.has(today) ? today : addDays(today, -1); active.has(date); date = addDays(date, -1)) current++;
  let longest = 0;
  for (const date of active) {
    if (active.has(addDays(date, -1))) continue;
    let length = 1;
    while (active.has(addDays(date, length))) length++;
    longest = Math.max(longest, length);
  }
  return { current, longest };
}

/** 0 for no activity, then 1-4 by share of the busiest day. */
export function intensity(day: ActivityDay, max: number) {
  const n = day.marked + day.read;
  return n === 0 || max === 0 ? 0 : Math.max(1, Math.ceil((n / max) * 4));
}

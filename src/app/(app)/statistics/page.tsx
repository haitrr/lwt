import Link from 'next/link';
import { cookies } from 'next/headers';
import { Plus } from 'lucide-react';
import { isLanguageCode, languageName } from '@/lib/languages';
import {
  buildCalendar,
  dateIn,
  intensity,
  streaks,
  TIME_ZONE_COOKIE,
  validTimeZone,
  type CalendarDay,
} from '@/lib/activity';
import { LearningLevel, LEVEL_LABELS, type TermCounts } from '@/lib/types';
import { requireSession } from '@/server/auth/session';
import { getActivity, getLanguageStatistics, type LanguageStatistics } from '@/server/statistics';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';

export const metadata = { title: 'Statistics' };

const LEARNING_LEVELS = [
  LearningLevel.Learning1,
  LearningLevel.Learning2,
  LearningLevel.Learning3,
  LearningLevel.Learning4,
  LearningLevel.Learning5,
];
const MARKED_LEVELS = [...LEARNING_LEVELS, LearningLevel.WellKnown];

// Phones show half a year so the cells stay big enough to see.
const WEEKS = 53;
const PHONE_WEEKS = 26;
const WEEKDAYS = ['Mon', '', 'Wed', '', 'Fri', '', ''];

const formatDay = new Intl.DateTimeFormat('en', { weekday: 'short', month: 'short', day: 'numeric', timeZone: 'UTC' });
const formatMonth = new Intl.DateTimeFormat('en', { month: 'short', timeZone: 'UTC' });
const utc = (date: string) => new Date(`${date}T00:00:00Z`);
const n = (value: number) => value.toLocaleString('en');
const plural = (value: number, word: string) => `${n(value)} ${value === 1 ? word : `${word}s`}`;
const sumOf = (counts: TermCounts, levels: LearningLevel[]) => levels.reduce((s, l) => s + (counts[l] ?? 0), 0);

function mergeLevels(languages: LanguageStatistics[]): TermCounts {
  const total: TermCounts = {};
  for (const { levels } of languages) {
    for (const [level, count] of Object.entries(levels) as [LearningLevel, number][]) {
      total[level] = (total[level] ?? 0) + count;
    }
  }
  return total;
}

function LanguageFilter({ languages, current }: { languages: string[]; current?: string }) {
  if (languages.length < 2) return null;
  const options = [{ code: undefined, label: 'All languages' }, ...languages.map((code) => ({ code, label: languageName(code) }))];
  return (
    <nav aria-label="Language" className="flex flex-wrap gap-2">
      {options.map((o) => (
        <Button
          key={o.code ?? 'all'}
          asChild
          size="sm"
          variant={o.code === current ? 'default' : 'outline'}
          className="rounded-full"
        >
          <Link href={o.code ? `/statistics?languageCode=${o.code}` : '/statistics'} aria-current={o.code === current ? 'page' : undefined}>
            {o.label}
          </Link>
        </Button>
      ))}
    </nav>
  );
}

function Tile({ label, value, hint }: { label: string; value: string; hint: string }) {
  return (
    <Card className="gap-1 py-5">
      <CardHeader className="gap-1">
        <CardDescription>{label}</CardDescription>
        <CardTitle className="text-3xl">{value}</CardTitle>
        <p className="text-xs text-muted-foreground">{hint}</p>
      </CardHeader>
    </Card>
  );
}

function dayLabel(day: CalendarDay) {
  const parts = [formatDay.format(utc(day.date))];
  if (!day.marked && !day.read) parts.push('no activity');
  if (day.marked) parts.push(`${plural(day.marked, 'word')} marked`);
  if (day.read) parts.push(`${plural(day.read, 'text')} read`);
  return parts.join(' · ');
}

function ActivityCalendar({ weeks }: { weeks: CalendarDay[][] }) {
  const days = weeks.flat().filter((d) => !d.future);
  const max = Math.max(0, ...days.map((d) => d.marked + d.read));
  // A week column is labeled with the month whose first day falls in it.
  const monthOf = (week: CalendarDay[]) => {
    const first = week.find((d) => d.date.endsWith('-01'));
    return first ? formatMonth.format(utc(first.date)) : '';
  };
  // Columns fill top to bottom: a month label, then Monday to Sunday.
  const phoneHidden = (w: number) => w < WEEKS - PHONE_WEEKS && 'hidden md:block';
  return (
    <div className="grid grid-flow-col grid-cols-[auto_repeat(26,minmax(0,1fr))] grid-rows-[auto_repeat(7,auto)] gap-[3px] text-[10px] leading-none text-muted-foreground md:grid-cols-[auto_repeat(53,minmax(0,1fr))]">
      <span />
      {WEEKDAYS.map((label, i) => (
        <span key={i} className="flex items-center pr-1.5">
          {label}
        </span>
      ))}
      {weeks.map((week, w) => [
        <span key={`month-${week[0].date}`} className={cn('pb-1 whitespace-nowrap', phoneHidden(w))}>
          {monthOf(week)}
        </span>,
        ...week.map((day) => (
          <span
            key={day.date}
            data-activity={day.future ? undefined : intensity(day, max)}
            title={day.future ? undefined : dayLabel(day)}
            className={cn('aspect-square rounded-[3px]', phoneHidden(w))}
          />
        )),
      ])}
    </div>
  );
}

function ActivityLegend() {
  return (
    <div className="flex items-center gap-1 text-xs text-muted-foreground" aria-hidden>
      <span className="mr-1">Less</span>
      {[0, 1, 2, 3, 4].map((step) => (
        <span key={step} data-activity={step} className="size-2.5 rounded-[3px]" />
      ))}
      <span className="ml-1">More</span>
    </div>
  );
}

function ActivityCard({ weeks }: { weeks: CalendarDay[][] }) {
  const days = weeks.flat().filter((d) => !d.future);
  const marked = days.reduce((s, d) => s + d.marked, 0);
  const read = days.reduce((s, d) => s + d.read, 0);
  const active = days.filter((d) => d.marked || d.read).length;
  const busiest = days.reduce<CalendarDay | null>((best, d) => (d.marked > (best?.marked ?? 0) ? d : best), null);
  const summary = `${plural(marked, 'word')} marked and ${plural(read, 'text')} read on ${plural(active, 'day')} in the past year.`;
  return (
    <Card className="gap-4">
      <CardHeader>
        <CardTitle>Activity</CardTitle>
        <CardDescription>
          <span className="md:hidden">Words marked and texts read, past 6 months</span>
          <span className="hidden md:inline">Words marked and texts read, past year</span>
        </CardDescription>
      </CardHeader>
      <CardContent role="img" aria-label={summary}>
        <ActivityCalendar weeks={weeks} />
      </CardContent>
      <CardFooter className="flex-wrap justify-between gap-x-6 gap-y-2 text-sm text-muted-foreground">
        <p>
          {summary}
          {busiest && ` Busiest day: ${formatDay.format(utc(busiest.date))}, ${plural(busiest.marked, 'word')}.`}
        </p>
        <ActivityLegend />
      </CardFooter>
    </Card>
  );
}

/** Proportion of each marked level, as one bar split into level-colored segments. */
function LevelBar({ levels, className }: { levels: TermCounts; className?: string }) {
  return (
    <div className={cn('flex h-2 gap-0.5 overflow-hidden rounded-full bg-muted', className)} aria-hidden>
      {MARKED_LEVELS.filter((l) => levels[l]).map((l) => (
        <span key={l} data-level={l} className="bg-level" style={{ flexGrow: levels[l] }} />
      ))}
    </div>
  );
}

function VocabularyCard({ levels }: { levels: TermCounts }) {
  const marked = sumOf(levels, MARKED_LEVELS);
  const max = Math.max(1, ...MARKED_LEVELS.map((l) => levels[l] ?? 0));
  const unknown = levels[LearningLevel.Unknown] ?? 0;
  return (
    <Card className="gap-4">
      <CardHeader>
        <CardTitle>Vocabulary by level</CardTitle>
        <CardDescription>{`${plural(marked, 'word')} marked`}</CardDescription>
      </CardHeader>
      <CardContent className="grid gap-4">
        {marked === 0 ? (
          <p className="text-sm text-muted-foreground">Mark words while reading to see them here.</p>
        ) : (
          <>
            <LevelBar levels={levels} className="h-3" />
            <table className="w-full text-sm">
              <tbody>
                {MARKED_LEVELS.map((level) => {
                  const count = levels[level] ?? 0;
                  return (
                    <tr key={level} data-level={level} className={cn(!count && 'opacity-50')}>
                      <th scope="row" className="py-1.5 pr-3 text-left font-normal whitespace-nowrap">
                        <span className="flex items-center gap-2 text-muted-foreground">
                          <span className="size-2.5 rounded-[3px] bg-level" />
                          {LEVEL_LABELS[level]}
                        </span>
                      </th>
                      <td className="w-full">
                        <span
                          className="block h-2 min-w-0.5 rounded-full bg-level"
                          style={{ width: `${(count / max) * 100}%` }}
                        />
                      </td>
                      <td className="pl-3 text-right font-medium tabular-nums">{n(count)}</td>
                      <td className="w-12 pl-2 text-right text-muted-foreground tabular-nums">
                        {`${Math.round((count / marked) * 100)}%`}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </>
        )}
      </CardContent>
      {unknown > 0 && (
        <CardFooter className="text-sm text-muted-foreground">
          {`${plural(unknown, 'more word')} seen in your texts but not marked yet.`}
        </CardFooter>
      )}
    </Card>
  );
}

function LanguagesCard({ languages }: { languages: LanguageStatistics[] }) {
  return (
    <Card className="gap-4">
      <CardHeader>
        <CardTitle>By language</CardTitle>
      </CardHeader>
      <CardContent>
        <table className="w-full text-sm">
          <thead className="text-xs text-muted-foreground">
            <tr>
              <th scope="col" className="pb-2 text-left font-normal">
                Language
              </th>
              <th scope="col" className="pb-2 pl-3 text-right font-normal">
                Texts
              </th>
              <th scope="col" className="pb-2 pl-3 text-right font-normal">
                Learning
              </th>
              <th scope="col" className="pb-2 pl-3 text-right font-normal whitespace-nowrap">
                Well known
              </th>
            </tr>
          </thead>
          <tbody className="tabular-nums">
            {languages.map((l) => (
              <tr key={l.languageCode} className="border-t">
                <th scope="row" className="w-full py-2.5 text-left font-medium">
                  <Link href={`/statistics?languageCode=${l.languageCode}`} className="hover:underline">
                    {languageName(l.languageCode)}
                  </Link>
                  <LevelBar levels={l.levels} className="mt-2 h-1.5" />
                </th>
                <td className="pl-3 text-right">{n(l.texts)}</td>
                <td className="pl-3 text-right">{n(sumOf(l.levels, LEARNING_LEVELS))}</td>
                <td className="pl-3 text-right">{n(l.levels[LearningLevel.WellKnown] ?? 0)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </CardContent>
    </Card>
  );
}

function NoStatistics() {
  return (
    <Card>
      <CardContent className="flex flex-col items-center gap-4 py-10 text-center">
        <p className="text-muted-foreground">Add a text and start reading to see your progress here.</p>
        <Button asChild>
          <Link href="/text/new">
            <Plus />
            Add text
          </Link>
        </Button>
      </CardContent>
    </Card>
  );
}

export default async function StatisticsPage({ searchParams }: PageProps<'/statistics'>) {
  const params = await searchParams;
  const session = await requireSession();
  const timeZone = validTimeZone((await cookies()).get(TIME_ZONE_COOKIE)?.value);
  const languages = (await getLanguageStatistics(session.userId)).sort(
    (a, b) => sumOf(b.levels, MARKED_LEVELS) - sumOf(a.levels, MARKED_LEVELS) || b.texts - a.texts,
  );
  const codes = languages.map((l) => l.languageCode);
  const requested = typeof params.languageCode === 'string' ? params.languageCode : '';
  const languageCode = isLanguageCode(requested) && codes.includes(requested) ? requested : undefined;
  const shown = languageCode ? languages.filter((l) => l.languageCode === languageCode) : languages;

  const today = dateIn(timeZone);
  const activity = await getActivity(session, WEEKS * 7, timeZone, languageCode);
  const weeks = buildCalendar(activity, today, WEEKS);
  const streak = streaks(activity, today);

  const levels = mergeLevels(shown);
  const learning = sumOf(levels, LEARNING_LEVELS);
  const wellKnown = levels[LearningLevel.WellKnown] ?? 0;
  const marked = learning + wellKnown;
  const textCount = shown.reduce((s, l) => s + l.texts, 0);

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-3">
        <h1 className="text-2xl font-semibold tracking-tight">Statistics</h1>
        <LanguageFilter languages={codes} current={languageCode} />
      </div>
      {languages.length === 0 ? (
        <NoStatistics />
      ) : (
        <>
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            <Tile label="Learning" value={n(learning)} hint="Words at levels 1–5" />
            <Tile
              label="Well known"
              value={n(wellKnown)}
              hint={marked ? `${Math.round((wellKnown / marked) * 100)}% of marked words` : 'No words marked yet'}
            />
            <Tile
              label="Day streak"
              value={plural(streak.current, 'day')}
              hint={`Best: ${plural(streak.longest, 'day')}`}
            />
            <Tile
              label="Texts"
              value={n(textCount)}
              hint={languageCode ? `In ${languageName(languageCode)}` : plural(codes.length, 'language')}
            />
          </div>
          <ActivityCard weeks={weeks} />
          <div className={cn('grid items-start gap-4', !languageCode && 'lg:grid-cols-2')}>
            <VocabularyCard levels={levels} />
            {!languageCode && <LanguagesCard languages={languages} />}
          </div>
        </>
      )}
    </div>
  );
}

import Link from 'next/link';
import { requireSession } from '@/server/auth/session';
import { LANGUAGES, languageName } from '@/lib/languages';
import { getTermCounts, listTexts } from '@/server/texts';
import { LearningLevel, LEVEL_LABELS, type TermCounts, type TextItem } from '@/lib/types';
import { BookOpen, ChevronLeft, ChevronRight, Pencil, Plus } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import TextFilter from './TextFilter';
import DeleteTextButton from './DeleteTextButton';
import RefreshWhileProcessing from './RefreshWhileProcessing';

export const metadata = { title: 'Texts' };

const ITEMS_PER_PAGE = 10;

const LEVEL_COLUMNS: [string, LearningLevel][] = [
  ['UK', LearningLevel.Unknown],
  ['L1', LearningLevel.Learning1],
  ['L2', LearningLevel.Learning2],
  ['L3', LearningLevel.Learning3],
  ['L4', LearningLevel.Learning4],
  ['L5', LearningLevel.Learning5],
  ['WK', LearningLevel.WellKnown],
];

const relativeTime = new Intl.RelativeTimeFormat('en', { numeric: 'auto' });

function timeAgo(iso: string) {
  const date = new Date(iso);
  const days = Math.round((date.getTime() - Date.now()) / 86_400_000);
  if (Math.abs(days) >= 30) return relativeTime.format(Math.round(days / 30), 'month');
  return relativeTime.format(days, 'day');
}

function percentRead(text: TextItem) {
  if (!text.termCount || !text.bookmark) return 0;
  return Math.floor((text.bookmark / text.termCount) * 10000) / 100;
}

function Status({ text }: { text: TextItem }) {
  if (text.termCount === 0)
    return (
      <Badge variant="outline" className="border-destructive/30 bg-destructive/10 text-destructive">
        Processing
      </Badge>
    );
  if (text.processedTermCount >= text.termCount)
    return (
      <Badge variant="outline" className="border-emerald-500/30 bg-emerald-500/10 text-emerald-700 dark:text-emerald-400">
        Done
      </Badge>
    );
  const pct = Math.floor((text.processedTermCount * 100) / text.termCount);
  return (
    <Badge variant="outline" className="border-amber-500/30 bg-amber-500/10 text-amber-700 dark:text-amber-400">
      {`${text.processedTermCount}/${text.termCount} (${pct}%)`}
    </Badge>
  );
}

/** How the text's words split across learning levels: a stacked bar, then the count and share of each level. */
function Vocabulary({ counts }: { counts: TermCounts }) {
  const total = Object.entries(counts)
    .filter(([l]) => l !== LearningLevel.Skipped && l !== LearningLevel.Ignored)
    .reduce((sum, [, n]) => sum + (n ?? 0), 0);
  const levels = LEVEL_COLUMNS.map(([label, level]) => ({ label, level, count: counts[level] ?? 0 }));
  const share = (n: number) => (total ? Math.round((n / total) * 100) : 0);
  return (
    <div className="flex min-w-0 flex-col gap-2">
      <div className="flex h-2 gap-0.5 overflow-hidden rounded-full bg-muted" aria-hidden>
        {levels
          .filter((l) => l.count)
          .map((l) => (
            <span key={l.level} data-level={l.level} className="bg-level" style={{ flexGrow: l.count }} />
          ))}
      </div>
      <ul className="flex flex-wrap gap-x-3 gap-y-1 text-xs tabular-nums">
        {levels.map((l) => (
          <li
            key={l.level}
            data-level={l.level}
            className={cn('flex items-center gap-1', !l.count && 'opacity-40')}
            title={LEVEL_LABELS[l.level]}
          >
            <span className="size-2 rounded-[2px] bg-level" />
            <span className="font-medium text-muted-foreground">{l.label}</span>
            <span>{l.count}</span>
            {l.count > 0 && <span className="text-muted-foreground">{`${share(l.count)}%`}</span>}
          </li>
        ))}
        <li className="flex items-center gap-1 text-muted-foreground" title="Ignored">
          <span className="font-medium">Ignored</span>
          <span>{counts[LearningLevel.Ignored] ?? 0}</span>
        </li>
      </ul>
    </div>
  );
}

export default async function TextsPage({ searchParams }: PageProps<'/text'>) {
  const params = await searchParams;
  const page = Math.max(1, Number(params.page) || 1);
  const title = typeof params.title === 'string' ? params.title : '';
  const languageCode = typeof params.languageCode === 'string' ? params.languageCode : '';

  const session = await requireSession();
  const texts = await listTexts(session.userId, { title, languageCode }, page, ITEMS_PER_PAGE);
  const counts = await Promise.all(texts.items.map((t) => getTermCounts(t.id, session.userId)));
  const pageCount = Math.max(1, Math.ceil(texts.total / ITEMS_PER_PAGE));
  const pageHref = (p: number) => {
    const q = new URLSearchParams({ page: String(p) });
    if (title) q.set('title', title);
    if (languageCode) q.set('languageCode', languageCode);
    return `/text?${q}`;
  };

  return (
    <div className="flex flex-col gap-4">
      <RefreshWhileProcessing
        active={texts.items.some((t) => t.termCount === 0 || t.processedTermCount < t.termCount)}
      />
      <div className="flex items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold tracking-tight">Texts</h1>
        <Button asChild>
          <Link href="/text/new">
            <Plus />
            Add text
          </Link>
        </Button>
      </div>
      <TextFilter languages={LANGUAGES} title={title} languageCode={languageCode} />
      {texts.items.length === 0 ? (
        <Card>
          <CardContent className="py-10 text-center text-muted-foreground">
            {title || languageCode ? 'No texts match this filter.' : 'No texts yet.'}
          </CardContent>
        </Card>
      ) : (
        <ul className="flex flex-col gap-3">
          {texts.items.map((text, i) => (
            <li key={text.id}>
              <Card className="grid gap-4 px-5 py-4 transition-colors hover:border-foreground/20 md:grid-cols-[minmax(0,1fr)_minmax(0,1.1fr)_auto] md:items-center md:gap-6">
                <div className="flex min-w-0 flex-col gap-2">
                  <div className="flex min-w-0 items-center gap-2">
                    <Link href={`/text/read/${text.id}`} className="truncate text-base font-semibold hover:underline">
                      {text.title}
                    </Link>
                    <Status text={text} />
                  </div>
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Badge variant="secondary">{languageName(text.languageCode)}</Badge>
                    <span>{`Created ${timeAgo(text.createdAt)}`}</span>
                  </div>
                  <div className="grid grid-cols-[1fr_auto] items-center gap-3 text-sm tabular-nums">
                    <Progress value={percentRead(text)} aria-label="Read" className="h-1.5" />
                    <Link href={`/text/read/${text.id}`} className="text-muted-foreground hover:text-foreground">
                      {`${percentRead(text)}% read`}
                    </Link>
                  </div>
                </div>
                <Vocabulary counts={counts[i]} />
                <div className="flex items-center gap-1 border-t pt-3 md:border-0 md:pt-0">
                  <Button asChild size="sm" className="mr-auto md:mr-0">
                    <Link href={`/text/read/${text.id}`}>
                      <BookOpen />
                      Read
                    </Link>
                  </Button>
                  <Button asChild variant="ghost" size="icon-sm" aria-label={`Edit ${text.title}`} title="Edit">
                    <Link href={`/text/${text.id}/edit`}>
                      <Pencil />
                    </Link>
                  </Button>
                  <DeleteTextButton id={text.id} title={text.title} />
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}
      {pageCount > 1 && (
        <nav className="flex items-center justify-between gap-4 text-sm" aria-label="Pages">
          {page > 1 ? (
            <Button asChild variant="outline" size="sm">
              <Link href={pageHref(page - 1)}>
                <ChevronLeft />
                Previous
              </Link>
            </Button>
          ) : (
            <span />
          )}
          <span className="text-muted-foreground">{`Page ${page} of ${pageCount}`}</span>
          {page < pageCount ? (
            <Button asChild variant="outline" size="sm">
              <Link href={pageHref(page + 1)}>
                Next
                <ChevronRight />
              </Link>
            </Button>
          ) : (
            <span />
          )}
        </nav>
      )}
    </div>
  );
}

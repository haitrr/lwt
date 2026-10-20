import Link from 'next/link';
import { redirect } from 'next/navigation';
import { requireSession } from '@/server/auth/session';
import { LANGUAGES, languageName } from '@/lib/languages';
import { countTextsByLanguage, getTermCountsByText, listTexts } from '@/server/texts';
import { LearningLevel, TEXT_SORTS, type TermCounts, type TextItem, type TextSort } from '@/lib/types';
import { BookOpen, ChevronLeft, ChevronRight, LoaderCircle, Pencil, Plus, SearchX } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { percentRead, Vocabulary } from '@/components/TextProgress';
import TextFilter from './TextFilter';
import DeleteTextButton from './DeleteTextButton';
import RefreshWhileProcessing from './RefreshWhileProcessing';

export const metadata = { title: 'Texts' };

const ITEMS_PER_PAGE = 10;

const relativeTime = new Intl.RelativeTimeFormat('en', { numeric: 'auto' });

function timeAgo(iso: string) {
  const days = Math.round((new Date(iso).getTime() - Date.now()) / 86_400_000);
  if (Math.abs(days) >= 365) return relativeTime.format(Math.round(days / 365), 'year');
  if (Math.abs(days) >= 30) return relativeTime.format(Math.round(days / 30), 'month');
  return relativeTime.format(days, 'day');
}

function plural(n: number, word: string) {
  return `${n.toLocaleString('en')} ${n === 1 ? word : `${word}s`}`;
}

const isProcessing = (text: TextItem) =>
  !text.processingError && (text.termCount === 0 || text.processedTermCount < text.termCount);

function FailedBadge() {
  return (
    <Badge variant="outline" className="shrink-0 border-destructive/30 bg-destructive/10 text-destructive">
      Failed
    </Badge>
  );
}

function ProcessingBadge({ text }: { text: TextItem }) {
  const pct = text.termCount ? Math.floor((text.processedTermCount * 100) / text.termCount) : 0;
  return (
    <Badge variant="outline" className="shrink-0 border-amber-500/30 bg-amber-500/10 text-amber-700 dark:text-amber-400">
      <LoaderCircle className="animate-spin" />
      {pct ? `Processing ${pct}%` : 'Processing'}
    </Badge>
  );
}

function ReadProgress({ text }: { text: TextItem }) {
  const pct = percentRead(text);
  return (
    <div className="grid grid-cols-[1fr_auto] items-center gap-3 text-sm tabular-nums">
      <Progress value={pct} aria-label="Read" className="h-1.5" />
      <span className="w-20 text-right text-muted-foreground">{text.bookmark == null ? 'Not started' : `${Math.floor(pct)}% read`}</span>
    </div>
  );
}

function TextCard({ text, counts }: { text: TextItem; counts: TermCounts }) {
  const processing = isProcessing(text);
  const started = text.bookmark != null;
  const words = Object.entries(counts)
    .filter(([level]) => level !== LearningLevel.Skipped)
    .reduce((sum, [, n]) => sum + (n ?? 0), 0);
  return (
    <Card className="grid gap-4 px-5 py-4 transition-colors hover:border-foreground/20 md:grid-cols-[minmax(0,1fr)_minmax(0,1.1fr)_auto] md:items-center md:gap-6">
      <div className="flex min-w-0 flex-col gap-2">
        <div className="flex min-w-0 items-center gap-2">
          <Link href={`/text/read/${text.id}`} className="truncate text-base font-semibold hover:underline" title={text.title}>
            {text.title}
          </Link>
          {processing && <ProcessingBadge text={text} />}
          {text.processingError && <FailedBadge />}
        </div>
        <div className="flex flex-wrap items-center gap-x-2 gap-y-1 text-sm text-muted-foreground">
          <Badge variant="secondary">{languageName(text.languageCode)}</Badge>
          {words > 0 && (
            <>
              <span className="tabular-nums">{plural(words, 'word')}</span>
              <span aria-hidden>·</span>
            </>
          )}
          <span>{started ? `Read ${timeAgo(text.lastReadAt)}` : `Added ${timeAgo(text.createdAt)}`}</span>
        </div>
        <ReadProgress text={text} />
      </div>
      {text.processingError ? (
        <p className="text-sm break-words text-destructive">
          {`Processing failed (edit the text to retry): ${text.processingError}`}
        </p>
      ) : text.termCount === 0 ? (
        <p className="text-sm text-muted-foreground">Splitting this text into words…</p>
      ) : (
        <Vocabulary counts={counts} />
      )}
      <div className="flex items-center gap-1 border-t pt-3 md:border-0 md:pt-0">
        <Button asChild size="sm" className="mr-auto min-w-24 md:mr-0">
          <Link href={`/text/read/${text.id}`}>
            <BookOpen />
            {started ? 'Continue' : 'Start'}
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
  );
}

function EmptyState({ filtered }: { filtered: boolean }) {
  const Icon = filtered ? SearchX : BookOpen;
  return (
    <Card className="items-center gap-3 px-6 py-12 text-center">
      <span className="grid size-10 place-items-center rounded-full bg-muted text-muted-foreground">
        <Icon className="size-5" />
      </span>
      <div className="grid gap-1">
        <h2 className="font-medium">{filtered ? 'No texts match' : 'No texts yet'}</h2>
        <p className="text-sm text-muted-foreground">
          {filtered
            ? 'Try another title or language.'
            : 'Add an article, story or transcript in the language you are learning.'}
        </p>
      </div>
      {!filtered && (
        <Button asChild className="mt-2">
          <Link href="/text/new">
            <Plus />
            Add text
          </Link>
        </Button>
      )}
    </Card>
  );
}

export default async function TextsPage({ searchParams }: PageProps<'/text'>) {
  const params = await searchParams;
  const page = Math.max(1, Math.floor(Number(params.page)) || 1);
  const title = typeof params.title === 'string' ? params.title : '';
  const languageCode = typeof params.languageCode === 'string' ? params.languageCode : '';
  const sort: TextSort = TEXT_SORTS.find((s) => s === params.sort) ?? 'recent';
  const query = (p: number) => {
    const q = new URLSearchParams();
    if (title) q.set('title', title);
    if (languageCode) q.set('languageCode', languageCode);
    if (sort !== 'recent') q.set('sort', sort);
    if (p > 1) q.set('page', String(p));
    return q.size ? `/text?${q}` : '/text';
  };

  const session = await requireSession();
  const [texts, textCounts] = await Promise.all([
    listTexts(session.userId, { title, languageCode }, page, ITEMS_PER_PAGE, sort),
    countTextsByLanguage(session.userId),
  ]);
  const pageCount = Math.max(1, Math.ceil(texts.total / ITEMS_PER_PAGE));
  if (page > pageCount) redirect(query(pageCount));
  const counts = await getTermCountsByText(
    texts.items.map((t) => t.id),
    session.userId,
  );

  const totalTexts = Object.values(textCounts).reduce((a, b) => a + b, 0);
  // Only languages that have texts, plus the selected one so the filter can always show it.
  const languages = LANGUAGES.filter((l) => textCounts[l.code] || l.code === languageCode).map((l) => ({
    ...l,
    count: textCounts[l.code] ?? 0,
  }));
  const first = (page - 1) * ITEMS_PER_PAGE + 1;
  const last = first + texts.items.length - 1;

  return (
    <div className="flex flex-col gap-4">
      <RefreshWhileProcessing active={texts.items.some(isProcessing)} />
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Texts</h1>
          {totalTexts > 0 && (
            <p className="text-muted-foreground">
              {`${plural(totalTexts, 'text')} in ${plural(Object.keys(textCounts).length, 'language')}`}
            </p>
          )}
        </div>
        {totalTexts > 0 && (
          <Button asChild>
            <Link href="/text/new">
              <Plus />
              Add text
            </Link>
          </Button>
        )}
      </div>
      {totalTexts > 0 && <TextFilter languages={languages} title={title} languageCode={languageCode} sort={sort} />}
      {texts.items.length === 0 ? (
        <EmptyState filtered={totalTexts > 0} />
      ) : (
        <ul className="flex flex-col gap-3">
          {texts.items.map((text) => (
            <li key={text.id}>
              <TextCard text={text} counts={counts.get(text.id)!} />
            </li>
          ))}
        </ul>
      )}
      {texts.total > 0 && (
        <nav className="flex items-center justify-between gap-4 text-sm" aria-label="Pages">
          {page > 1 ? (
            <Button asChild variant="outline" size="sm">
              <Link href={query(page - 1)}>
                <ChevronLeft />
                Previous
              </Link>
            </Button>
          ) : (
            <span />
          )}
          <span className="text-muted-foreground tabular-nums">
            {pageCount > 1 ? `${first}–${last} of ${texts.total}` : plural(texts.total, 'text')}
          </span>
          {page < pageCount ? (
            <Button asChild variant="outline" size="sm">
              <Link href={query(page + 1)}>
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

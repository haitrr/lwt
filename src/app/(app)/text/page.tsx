import Link from 'next/link';
import { requireSession } from '@/server/auth/session';
import { LANGUAGES, languageName } from '@/lib/languages';
import { getTermCountsForTexts, listTexts } from '@/server/texts';
import type { TextItem } from '@/lib/types';
import { BookOpen, ChevronLeft, ChevronRight, Pencil, Plus } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { percentRead, Vocabulary } from '@/components/TextProgress';
import TextFilter from './TextFilter';
import DeleteTextButton from './DeleteTextButton';
import RefreshWhileProcessing from './RefreshWhileProcessing';

export const metadata = { title: 'Texts' };

const ITEMS_PER_PAGE = 10;

const relativeTime = new Intl.RelativeTimeFormat('en', { numeric: 'auto' });

function timeAgo(iso: string) {
  const date = new Date(iso);
  const days = Math.round((date.getTime() - Date.now()) / 86_400_000);
  if (Math.abs(days) >= 30) return relativeTime.format(Math.round(days / 30), 'month');
  return relativeTime.format(days, 'day');
}

function Status({ text }: { text: TextItem }) {
  if (text.processingError)
    return (
      <Badge variant="outline" className="border-destructive/30 bg-destructive/10 text-destructive">
        Failed
      </Badge>
    );
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

export default async function TextsPage({ searchParams }: PageProps<'/text'>) {
  const params = await searchParams;
  const page = Math.max(1, Number(params.page) || 1);
  const title = typeof params.title === 'string' ? params.title : '';
  const languageCode = typeof params.languageCode === 'string' ? params.languageCode : '';

  const session = await requireSession();
  const texts = await listTexts(session.userId, { title, languageCode }, page, ITEMS_PER_PAGE);
  const counts = await getTermCountsForTexts(
    texts.items.map((t) => t.id),
    session.userId,
  );
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
        active={texts.items.some(
          (t) => !t.processingError && (t.termCount === 0 || t.processedTermCount < t.termCount),
        )}
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
          {texts.items.map((text) => (
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
                  {text.processingError && (
                    <p className="text-sm break-words text-destructive">
                      {`Processing failed (edit the text to retry): ${text.processingError}`}
                    </p>
                  )}
                  <div className="grid grid-cols-[1fr_auto] items-center gap-3 text-sm tabular-nums">
                    <Progress value={percentRead(text)} aria-label="Read" className="h-1.5" />
                    <Link href={`/text/read/${text.id}`} className="text-muted-foreground hover:text-foreground">
                      {`${percentRead(text)}% read`}
                    </Link>
                  </div>
                </div>
                <Vocabulary counts={counts.get(text.id)!} />
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

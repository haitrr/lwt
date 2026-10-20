import Link from 'next/link';
import { ArrowRight, BookOpen, ChartColumn, Pencil, Plus } from 'lucide-react';
import { languageName } from '@/lib/languages';
import { LearningLevel, type TextItem } from '@/lib/types';
import { requireSession } from '@/server/auth/session';
import { countTermsByLearningLevel } from '@/server/terms';
import { countTextsByLanguage, getLastReadText, getTermCounts, listTexts } from '@/server/texts';
import { percentRead, Vocabulary } from '@/components/TextProgress';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';

export const metadata = { title: 'Home' };

const RECENT_COUNT = 6;

const STEPS = [
  { title: 'Add a text', body: 'Paste an article, story or transcript in the language you are learning.' },
  { title: 'Read and mark words', body: 'Click a word to look it up, save its meaning and set how well you know it.' },
  { title: 'Watch it grow', body: 'Every text you read updates your vocabulary across all your other texts.' },
];

function AddTextButton({ variant }: { variant?: 'outline' }) {
  return (
    <Button asChild variant={variant}>
      <Link href="/text/new">
        <Plus />
        Add text
      </Link>
    </Button>
  );
}

function GetStarted({ userName }: { userName: string }) {
  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{`Welcome, ${userName}`}</h1>
        <p className="text-muted-foreground">Learn a language by reading texts you care about.</p>
      </div>
      <Card className="max-w-3xl">
        <CardHeader>
          <CardTitle className="text-lg">Get started</CardTitle>
          <CardDescription>Three steps to your first reading session.</CardDescription>
        </CardHeader>
        <CardContent>
          <ol className="grid gap-5 sm:grid-cols-3">
            {STEPS.map((step, i) => (
              <li key={step.title} className="flex gap-3 sm:flex-col">
                <span className="grid size-7 shrink-0 place-items-center rounded-full bg-primary text-sm font-semibold text-primary-foreground">
                  {i + 1}
                </span>
                <div className="grid gap-1">
                  <h2 className="font-medium">{step.title}</h2>
                  <p className="text-sm text-muted-foreground">{step.body}</p>
                </div>
              </li>
            ))}
          </ol>
        </CardContent>
        <CardFooter>
          <AddTextButton />
        </CardFooter>
      </Card>
    </div>
  );
}

function ReadProgress({ text }: { text: TextItem }) {
  if (text.processingError) {
    return (
      <p className="text-sm break-words text-destructive">
        {`Splitting this text into words failed: ${text.processingError} — `}
        <Link href={`/text/${text.id}/edit`} className="font-medium underline underline-offset-2">
          Edit the text to retry
        </Link>
      </p>
    );
  }
  if (text.termCount === 0) {
    return <p className="text-sm text-muted-foreground">Still splitting this text into words…</p>;
  }
  const pct = percentRead(text);
  return (
    <div className="grid gap-2">
      <div className="flex items-baseline justify-between gap-3 text-sm tabular-nums">
        <span className="font-medium">{`${pct}% read`}</span>
        <span className="text-muted-foreground">{`${(text.bookmark ?? 0).toLocaleString('en')} / ${text.termCount.toLocaleString('en')} terms`}</span>
      </div>
      <Progress value={pct} aria-label="Read" />
    </div>
  );
}

async function ContinueReading({ text, started, userId }: { text: TextItem; started: boolean; userId: number }) {
  const counts = await getTermCounts(text.id, userId);
  const unknown = counts[LearningLevel.Unknown] ?? 0;
  return (
    <Card className="gap-5">
      <CardHeader>
        <CardDescription className="text-xs font-medium tracking-wider uppercase">
          {started ? 'Continue reading' : 'Start reading'}
        </CardDescription>
        <CardTitle className="text-xl leading-snug break-words">
          <Link href={`/text/read/${text.id}`} className="hover:underline">
            {text.title}
          </Link>
        </CardTitle>
        <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
          <Badge variant="secondary">{languageName(text.languageCode)}</Badge>
          {unknown > 0 && <span>{`${unknown.toLocaleString('en')} new ${unknown === 1 ? 'word' : 'words'} to learn`}</span>}
        </div>
      </CardHeader>
      <CardContent className="grid gap-5">
        <ReadProgress text={text} />
        {text.termCount > 0 && !text.processingError && <Vocabulary counts={counts} />}
      </CardContent>
      <CardFooter className="gap-2">
        <Button asChild className="flex-1 sm:flex-none">
          <Link href={`/text/read/${text.id}`}>
            <BookOpen />
            {started ? 'Continue reading' : 'Start reading'}
          </Link>
        </Button>
        <Button asChild variant="ghost" size="icon" aria-label={`Edit ${text.title}`} title="Edit">
          <Link href={`/text/${text.id}/edit`}>
            <Pencil />
          </Link>
        </Button>
      </CardFooter>
    </Card>
  );
}

async function Overview({ userId }: { userId: number }) {
  const [textCounts, levelCounts] = await Promise.all([countTextsByLanguage(userId), countTermsByLearningLevel(userId)]);
  const learned = Object.values(levelCounts).reduce((a, b) => a + b, 0);
  const wellKnown = levelCounts[LearningLevel.WellKnown] ?? 0;
  const tiles = [
    { label: 'Texts', value: Object.values(textCounts).reduce((a, b) => a + b, 0) },
    { label: 'Languages', value: Object.keys(textCounts).length },
    { label: 'Learning', value: learned - wellKnown },
    { label: 'Well known', value: wellKnown },
  ];
  return (
    <Card className="gap-4">
      <CardHeader>
        <CardTitle>Your progress</CardTitle>
      </CardHeader>
      <CardContent>
        <dl className="grid grid-cols-2 gap-x-4 gap-y-5">
          {tiles.map((t) => (
            <div key={t.label} className="grid gap-1">
              <dt className="text-sm text-muted-foreground">{t.label}</dt>
              <dd className="text-2xl font-semibold tabular-nums">{t.value.toLocaleString('en')}</dd>
            </div>
          ))}
        </dl>
      </CardContent>
      <CardFooter>
        <Button asChild variant="outline" size="sm">
          <Link href="/statistics">
            <ChartColumn />
            View statistics
          </Link>
        </Button>
      </CardFooter>
    </Card>
  );
}

function RecentText({ text }: { text: TextItem }) {
  const pct = percentRead(text);
  return (
    <Card className="relative gap-3 px-5 py-4 transition-colors hover:border-foreground/20">
      <Link href={`/text/read/${text.id}`} className="truncate font-medium after:absolute after:inset-0 after:rounded-xl">
        {text.title}
      </Link>
      <div className="flex items-center justify-between gap-2 text-sm text-muted-foreground tabular-nums">
        <Badge variant="secondary">{languageName(text.languageCode)}</Badge>
        {text.processingError ? (
          <span className="text-destructive">Failed</span>
        ) : (
          <span>{text.termCount === 0 ? 'Processing' : pct === 0 ? 'Not started' : `${pct}% read`}</span>
        )}
      </div>
      <Progress value={pct} aria-label="Read" className="h-1.5" />
    </Card>
  );
}

export default async function HomePage() {
  const session = await requireSession();
  const [lastRead, recent] = await Promise.all([
    getLastReadText(session),
    listTexts(session.userId, {}, 1, RECENT_COUNT + 1),
  ]);
  const current = lastRead ?? recent.items[0];
  if (!current) return <GetStarted userName={session.userName} />;
  const others = recent.items.filter((t) => t.id !== current.id).slice(0, RECENT_COUNT);
  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">{`Welcome back, ${session.userName}`}</h1>
          <p className="text-muted-foreground">{lastRead ? 'Pick up where you left off.' : 'Ready for your first reading session?'}</p>
        </div>
        <AddTextButton variant="outline" />
      </div>
      {/* Phones show recent texts before the overview; wide screens put the overview in a side column. */}
      <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_20rem]">
        <ContinueReading text={current} started={!!lastRead} userId={session.userId} />
        {others.length > 0 && (
          <section className="flex flex-col gap-3 lg:col-start-1" aria-labelledby="recent-texts">
            <div className="flex items-center justify-between gap-4">
              <h2 id="recent-texts" className="text-lg font-semibold tracking-tight">
                Recent texts
              </h2>
              <Button asChild variant="ghost" size="sm" className="text-muted-foreground">
                <Link href="/text">
                  All texts
                  <ArrowRight />
                </Link>
              </Button>
            </div>
            <ul className="grid gap-3 sm:grid-cols-2">
              {others.map((text) => (
                <li key={text.id} className="min-w-0">
                  <RecentText text={text} />
                </li>
              ))}
            </ul>
          </section>
        )}
        <div className="lg:col-start-2 lg:row-span-2 lg:row-start-1">
          <Overview userId={session.userId} />
        </div>
      </div>
    </div>
  );
}

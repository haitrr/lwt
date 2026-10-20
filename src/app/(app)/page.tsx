import Link from 'next/link';
import { BookOpen, Plus } from 'lucide-react';
import { languageName } from '@/lib/languages';
import { requireSession } from '@/server/auth/session';
import { getLastReadText } from '@/server/texts';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';

export const metadata = { title: 'Home' };

export default async function HomePage() {
  const session = await requireSession();
  const text = await getLastReadText(session);
  if (!text) {
    return (
      <Card className="max-w-xl">
        <CardHeader>
          <CardTitle className="text-2xl">{`Welcome, ${session.userName}`}</CardTitle>
          <CardDescription>Nothing read yet. Add a text or pick one to start reading.</CardDescription>
        </CardHeader>
        <CardFooter className="gap-2">
          <Button asChild>
            <Link href="/text/new">
              <Plus />
              Add text
            </Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/text">
              <BookOpen />
              Browse texts
            </Link>
          </Button>
        </CardFooter>
      </Card>
    );
  }
  const read = text.bookmark ?? 0;
  const percentage = text.termCount ? ((read * 100) / text.termCount).toFixed(2) : '0.00';
  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-2xl font-semibold tracking-tight">{`Welcome back, ${session.userName}`}</h1>
      <Card className="max-w-xl">
        <CardHeader>
          <CardDescription className="text-xs font-medium tracking-wider uppercase">Previously read</CardDescription>
          <CardTitle className="text-xl">{text.title}</CardTitle>
          <Badge variant="secondary">{languageName(text.languageCode)}</Badge>
        </CardHeader>
        <CardContent className="grid gap-2">
          <Progress value={Number(percentage)} aria-label="Read" />
          <p className="text-sm text-muted-foreground">{`Done: ${read}/${text.termCount} (${percentage}%)`}</p>
        </CardContent>
        <CardFooter className="gap-2">
          <Button asChild>
            <Link href={`/text/read/${text.id}`}>
              <BookOpen />
              Continue reading
            </Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/text">All texts</Link>
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}

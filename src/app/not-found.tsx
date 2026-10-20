import Link from 'next/link';
import { FileQuestion, Home } from 'lucide-react';
import Brand from '@/components/Brand';
import StatusMessage from '@/components/StatusMessage';
import { Button } from '@/components/ui/button';

export const metadata = { title: 'Not found' };

// Any URL that matches no route; it renders outside the app header, like the login page.
export default function NotFound() {
  return (
    <main className="flex min-h-dvh flex-col items-center justify-center gap-6 bg-muted p-4">
      <Brand className="text-lg" />
      <StatusMessage icon={FileQuestion} title="Page not found" description="There's nothing at this address.">
        <Button asChild>
          <Link href="/">
            <Home />
            Home
          </Link>
        </Button>
      </StatusMessage>
    </main>
  );
}

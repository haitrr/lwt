import Link from 'next/link';
import { BookOpen, FileQuestion } from 'lucide-react';
import StatusMessage from '@/components/StatusMessage';
import { Button } from '@/components/ui/button';

// Shown when a page under the app header calls notFound(), e.g. a text that doesn't exist or isn't yours.
export default function AppNotFound() {
  // Only the root not-found can export metadata.
  return (
    <StatusMessage icon={FileQuestion} title="Not found" description="This page doesn't exist, or it belongs to another account.">
      <title>Not found · Lwt</title>
      <Button asChild>
        <Link href="/text">
          <BookOpen />
          Your texts
        </Link>
      </Button>
    </StatusMessage>
  );
}

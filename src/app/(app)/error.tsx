'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { CircleAlert, Home, RotateCw } from 'lucide-react';
import StatusMessage from '@/components/StatusMessage';
import { Button } from '@/components/ui/button';

// Pages under the app header that fail to render show this in place of the page, keeping the header.
export default function AppError({ error, retry }: { error: unknown; retry: () => void }) {
  useEffect(() => {
    console.error(error);
  }, [error]);
  return (
    <StatusMessage
      icon={CircleAlert}
      tone="destructive"
      title="Something went wrong"
      description="This page couldn't be loaded. It may be a temporary problem; try again in a moment."
    >
      <Button onClick={() => retry()}>
        <RotateCw />
        Try again
      </Button>
      <Button asChild variant="outline">
        <Link href="/">
          <Home />
          Home
        </Link>
      </Button>
    </StatusMessage>
  );
}

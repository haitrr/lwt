'use client';

import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { advanceProcessing } from '@/lib/texts';

// New texts are split into terms in the background; push that along and refresh until every
// listed text is done.
export default function RefreshWhileProcessing({ active }: { active: boolean }) {
  const router = useRouter();
  useEffect(() => {
    if (!active) return;
    let stopped = false;
    const tick = async () => {
      await advanceProcessing().catch(() => undefined);
      if (stopped) return;
      router.refresh();
      setTimeout(tick, 1000);
    };
    void tick();
    return () => {
      stopped = true;
    };
  }, [active, router]);
  return null;
}

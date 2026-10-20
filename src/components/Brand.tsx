import Link from 'next/link';
import { cn } from '@/lib/utils';

export default function Brand({ className, hideName }: { className?: string; hideName?: boolean }) {
  return (
    <Link href="/" className={cn('flex items-center gap-2 font-semibold tracking-tight', className)} aria-label="Lwt home">
      <span className="grid size-7 place-items-center rounded-md bg-primary text-sm text-primary-foreground" aria-hidden>
        L
      </span>
      <span className={cn(hideName && 'hidden sm:inline')}>Lwt</span>
    </Link>
  );
}

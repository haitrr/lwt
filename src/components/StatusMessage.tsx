import type { LucideIcon } from 'lucide-react';
import { Card } from '@/components/ui/card';
import { cn } from '@/lib/utils';

/** A centered notice for pages that can't show their content: not found, failed to load. */
export default function StatusMessage({
  icon: Icon,
  title,
  description,
  tone = 'muted',
  children,
}: {
  icon: LucideIcon;
  title: string;
  description: string;
  tone?: 'muted' | 'destructive';
  children?: React.ReactNode;
}) {
  return (
    <Card className="mx-auto w-full max-w-lg items-center gap-4 px-6 py-12 text-center">
      <span
        className={cn(
          'grid size-10 place-items-center rounded-full',
          tone === 'destructive' ? 'bg-destructive/10 text-destructive' : 'bg-muted text-muted-foreground',
        )}
      >
        <Icon className="size-5" />
      </span>
      <div className="grid gap-1">
        <h1 className="font-medium">{title}</h1>
        <p className="text-sm text-muted-foreground">{description}</p>
      </div>
      {children && <div className="flex flex-wrap justify-center gap-2">{children}</div>}
    </Card>
  );
}

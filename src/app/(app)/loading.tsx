import { LoaderCircle } from 'lucide-react';

export default function Loading() {
  return (
    <p className="flex items-center gap-2 text-sm text-muted-foreground">
      <LoaderCircle className="size-4 animate-spin" />
      Loading…
    </p>
  );
}

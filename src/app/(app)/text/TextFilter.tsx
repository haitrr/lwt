'use client';

import { usePathname, useRouter } from 'next/navigation';
import { useEffect, useRef, useState, useTransition } from 'react';
import { LoaderCircle, Search, X } from 'lucide-react';
import type { Language, TextSort } from '@/lib/types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { NativeSelect, NativeSelectOption } from '@/components/ui/native-select';

const SORT_LABELS: Record<TextSort, string> = {
  recent: 'Recently read',
  newest: 'Newest',
  title: 'Title',
};

type Filter = { title: string; languageCode: string; sort: TextSort };
type Props = Filter & { languages: (Language & { count: number })[] };

export default function TextFilter({ languages, ...initial }: Props) {
  const router = useRouter();
  const pathname = usePathname();
  const [pending, startTransition] = useTransition();
  const [filter, setFilter] = useState<Filter>(initial);
  const typing = useRef<ReturnType<typeof setTimeout>>(undefined);
  useEffect(() => () => clearTimeout(typing.current), []);

  // A new filter always starts back on page 1.
  const apply = (next: Filter) => {
    clearTimeout(typing.current);
    const query = new URLSearchParams();
    if (next.title) query.set('title', next.title);
    if (next.languageCode) query.set('languageCode', next.languageCode);
    if (next.sort !== 'recent') query.set('sort', next.sort);
    startTransition(() => router.replace(query.size ? `${pathname}?${query}` : pathname));
  };

  // Typing waits for a pause; the selects apply at once.
  const change = (patch: Partial<Filter>, now: boolean) => {
    const next = { ...filter, ...patch };
    setFilter(next);
    if (now) apply(next);
    else {
      clearTimeout(typing.current);
      typing.current = setTimeout(() => apply(next), 400);
    }
  };
  const filtered = !!(filter.title || filter.languageCode);

  return (
    <div className="flex flex-wrap gap-2">
      <div className="relative min-w-0 basis-full sm:basis-60 sm:flex-1">
        {pending ? (
          <LoaderCircle className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 animate-spin text-muted-foreground" />
        ) : (
          <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
        )}
        <Input
          type="search"
          placeholder="Search titles"
          aria-label="Search titles"
          className="pl-9"
          value={filter.title}
          onChange={(e) => change({ title: e.target.value }, false)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') apply(filter);
          }}
        />
      </div>
      <NativeSelect
        aria-label="Language"
        className="min-w-0 flex-1 sm:w-44 sm:flex-none"
        value={filter.languageCode}
        onChange={(e) => change({ languageCode: e.target.value }, true)}
      >
        <NativeSelectOption value="">All languages</NativeSelectOption>
        {languages.map((l) => (
          <NativeSelectOption key={l.code} value={l.code}>
            {`${l.name} (${l.count})`}
          </NativeSelectOption>
        ))}
      </NativeSelect>
      <NativeSelect
        aria-label="Sort by"
        className="min-w-0 flex-1 sm:w-40 sm:flex-none"
        value={filter.sort}
        onChange={(e) => change({ sort: e.target.value as TextSort }, true)}
      >
        {Object.entries(SORT_LABELS).map(([value, label]) => (
          <NativeSelectOption key={value} value={value}>
            {label}
          </NativeSelectOption>
        ))}
      </NativeSelect>
      {filtered && (
        <Button
          variant="ghost"
          className="text-muted-foreground"
          onClick={() => change({ title: '', languageCode: '' }, true)}
        >
          <X />
          Clear
        </Button>
      )}
    </div>
  );
}

'use client';

import { usePathname, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import type { Language } from '@/lib/types';
import LanguageSelect from './LanguageSelect';
import { Search } from 'lucide-react';
import { Input } from '@/components/ui/input';

type Props = { languages: Language[]; title: string; languageCode: string };

export default function TextFilter({ languages, title: initialTitle, languageCode: initialLanguage }: Props) {
  const router = useRouter();
  const pathname = usePathname();
  const [title, setTitle] = useState(initialTitle);
  const [languageCode, setLanguageCode] = useState(initialLanguage);

  // Debounce like the old filter form; a new filter always starts back on page 1.
  useEffect(() => {
    if (title === initialTitle && languageCode === initialLanguage) return;
    const timeout = setTimeout(() => {
      const query = new URLSearchParams();
      if (title) query.set('title', title);
      if (languageCode) query.set('languageCode', languageCode);
      router.replace(`${pathname}?${query}`);
    }, 500);
    return () => clearTimeout(timeout);
  }, [title, languageCode, initialTitle, initialLanguage, pathname, router]);

  return (
    <div className="flex flex-wrap gap-2">
      <div className="relative min-w-60 flex-1">
        <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          type="search"
          placeholder="Filter by title"
          aria-label="Filter by title"
          className="pl-9"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
      </div>
      <LanguageSelect
        languages={languages}
        value={languageCode}
        onChange={(e) => setLanguageCode(e.target.value)}
        emptyLabel="All languages"
        aria-label="Language"
        className="w-44"
      />
    </div>
  );
}

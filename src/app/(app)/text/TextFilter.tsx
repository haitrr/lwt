'use client';

import { usePathname, useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';
import type { Language } from '@/lib/types';
import LanguageSelect from './LanguageSelect';

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
    <div className="filter">
      <LanguageSelect
        languages={languages}
        value={languageCode}
        onChange={(e) => setLanguageCode(e.target.value)}
        emptyLabel="All languages"
      />
      <input placeholder="Filter by title" value={title} onChange={(e) => setTitle(e.target.value)} />
    </div>
  );
}

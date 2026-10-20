import type { ComponentProps } from 'react';
import type { Language } from '@/lib/types';

type Props = ComponentProps<'select'> & { languages: Language[]; emptyLabel: string };

export default function LanguageSelect({ languages, emptyLabel, ...props }: Props) {
  return (
    <select {...props}>
      <option value="">{emptyLabel}</option>
      {languages.map((l) => (
        <option key={l.code} value={l.code}>
          {l.name}
        </option>
      ))}
    </select>
  );
}

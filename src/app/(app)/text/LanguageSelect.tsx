import type { ComponentProps } from 'react';
import type { Language } from '@/lib/types';
import { NativeSelect, NativeSelectOption } from '@/components/ui/native-select';

type Props = ComponentProps<typeof NativeSelect> & { languages: Language[]; emptyLabel: string };

export default function LanguageSelect({ languages, emptyLabel, ...props }: Props) {
  return (
    <NativeSelect {...props}>
      <NativeSelectOption value="">{emptyLabel}</NativeSelectOption>
      {languages.map((l) => (
        <NativeSelectOption key={l.code} value={l.code}>
          {l.name}
        </NativeSelectOption>
      ))}
    </NativeSelect>
  );
}

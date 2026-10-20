import type { ComponentProps } from 'react';
import type { Language } from '@/lib/types';
import { NativeSelect, NativeSelectOption } from '@/components/ui/native-select';

type Props = ComponentProps<typeof NativeSelect> & {
  languages: Language[];
  emptyLabel: string;
  /** Leave out languages texts can't be written in. */
  textsOnly?: boolean;
};

export default function LanguageSelect({ languages, emptyLabel, textsOnly, ...props }: Props) {
  return (
    <NativeSelect {...props}>
      <NativeSelectOption value="">{emptyLabel}</NativeSelectOption>
      {languages
        .filter((l) => !textsOnly || l.supportsTexts)
        .map((l) => (
          <NativeSelectOption key={l.code} value={l.code}>
            {l.name}
          </NativeSelectOption>
        ))}
    </NativeSelect>
  );
}

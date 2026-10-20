import type { DictionaryLookup } from '@/lib/types';
import { LoaderCircle } from 'lucide-react';

/** The dictionary's meaning, when the lookup found one. */
export function suggestionOf(lookup: DictionaryLookup | undefined) {
  return lookup?.status === 'found' ? lookup.meaning : null;
}

const MESSAGES = {
  loading: 'Looking up in dictionary…',
  found: 'From dictionary',
  missing: 'Not found in dictionary',
  unavailable: 'Dictionary unavailable',
} satisfies Record<DictionaryLookup['status'], string>;

/** One muted line saying where a lookup stands; nothing when no lookup ran. */
export default function DictionaryHint({ lookup }: { lookup: DictionaryLookup | undefined }) {
  if (!lookup) return null;
  return (
    <span className="flex items-center gap-1.5 text-xs text-muted-foreground" role="status">
      {lookup.status === 'loading' && <LoaderCircle className="size-3 animate-spin" />}
      {MESSAGES[lookup.status]}
    </span>
  );
}

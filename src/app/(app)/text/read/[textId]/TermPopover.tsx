'use client';

import { useState } from 'react';
import { importanceColor } from '@/lib/importance';
import { nextLevel, previousLevel, LEVEL_LABELS, type DictionaryLookup, type LearningLevel, type TextTermView } from '@/lib/types';
import { ArrowDown, ArrowUp } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import DictionaryHint, { suggestionOf } from './DictionaryHint';

type Props = {
  term: TextTermView;
  rect: DOMRect;
  lookup: DictionaryLookup | undefined;
  onKeep: () => void;
  onLeave: () => void;
  /** Resolves to null once saved, or to the error to show. */
  onChangeLevel: (level: LearningLevel) => Promise<string | null>;
};

export default function TermPopover({ term, rect, lookup, onKeep, onLeave, onChangeLevel }: Props) {
  const [changing, setChanging] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const meaning = term.meaning || suggestionOf(lookup);

  // One change at a time, so a double click cannot move the level twice from the same start.
  const changeLevel = async (level: LearningLevel) => {
    setChanging(true);
    setError(null);
    setError(await onChangeLevel(level));
    setChanging(false);
  };

  const above = rect.top > window.innerHeight / 2;
  return (
    <div
      className="fixed z-30 flex w-[300px] flex-col gap-2 rounded-lg border bg-popover p-4 text-sm text-popover-foreground shadow-lg animate-in fade-in-0 zoom-in-95"
      role="dialog"
      style={{
        left: Math.min(rect.left, window.innerWidth - 320),
        ...(above ? { bottom: window.innerHeight - rect.top + 8 } : { top: rect.bottom + 8 }),
      }}
      onMouseEnter={onKeep}
      onMouseLeave={onLeave}
    >
      <div className="flex items-center justify-between gap-2">
        <strong className="text-xl">{term.content}</strong>
        <Badge variant="secondary" data-level={term.learningLevel}>
          <span className="size-2 rounded-full bg-level" />
          {LEVEL_LABELS[term.learningLevel]}
        </Badge>
      </div>
      {term.count ? (
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <span className="h-[3px] w-3 rounded-full" style={{ background: importanceColor(term.count) }} />
          {`${term.count} in this text`}
        </div>
      ) : null}
      {lookup?.status !== 'loading' && (
        <p className="whitespace-pre-line">{meaning || <em className="text-muted-foreground">No meaning yet</em>}</p>
      )}
      {!term.meaning && <DictionaryHint lookup={lookup} />}
      {error && (
        <p className="text-destructive" role="alert">
          {error}
        </p>
      )}
      <div className="grid grid-cols-2 gap-2">
        <Button size="sm" disabled={changing} onClick={() => void changeLevel(nextLevel(term.learningLevel))}>
          <ArrowUp />
          Better
        </Button>
        <Button
          size="sm"
          variant="outline"
          disabled={changing}
          onClick={() => void changeLevel(previousLevel(term.learningLevel))}
        >
          <ArrowDown />
          Worse
        </Button>
      </div>
    </div>
  );
}

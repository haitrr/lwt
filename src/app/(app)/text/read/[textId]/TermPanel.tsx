'use client';

import { useState } from 'react';
import { importanceColor } from '@/lib/importance';
import { LearningLevel, LEVEL_LABELS, nextLevel, type TextTermView } from '@/lib/types';
import { ArrowUp, Copy, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { NativeSelect, NativeSelectOption } from '@/components/ui/native-select';
import { Textarea } from '@/components/ui/textarea';

type Props = {
  term: TextTermView;
  suggestion: string | null;
  onSave: (level: LearningLevel, meaning: string) => Promise<void>;
  onClose: () => void;
};

const EDITABLE_LEVELS = Object.values(LearningLevel).filter((l) => l !== LearningLevel.Skipped);

export default function TermPanel({ term, suggestion, onSave, onClose }: Props) {
  const [meaning, setMeaning] = useState(term.meaning ?? '');
  const [level, setLevel] = useState(term.learningLevel);
  const [saving, setSaving] = useState(false);
  const shownMeaning = meaning || (term.meaning ? '' : (suggestion ?? ''));

  const save = async (nextLevelValue: LearningLevel) => {
    setSaving(true);
    try {
      await onSave(nextLevelValue, shownMeaning);
    } finally {
      setSaving(false);
    }
  };

  return (
    <form
      className="fixed inset-x-0 bottom-0 z-30 flex flex-col gap-3 rounded-t-xl border bg-popover p-4 pb-[max(1rem,env(safe-area-inset-bottom))] text-popover-foreground shadow-lg animate-in slide-in-from-bottom-4 sm:inset-x-auto sm:bottom-5 sm:left-1/2 sm:w-[34rem] sm:-translate-x-1/2 sm:rounded-xl"
      onSubmit={(e) => {
        e.preventDefault();
        void save(level);
      }}
    >
      <div className="flex items-center justify-between gap-2">
        <button
          type="button"
          className="flex items-center gap-1.5 text-xl font-bold hover:opacity-80 [&>svg]:size-3.5 [&>svg]:text-muted-foreground"
          title="Copy"
          style={{ color: term.count ? importanceColor(term.count) : undefined }}
          onClick={() => void navigator.clipboard?.writeText(term.content)}
        >
          {`${term.content} (${term.count ?? '-'})`}
          <Copy />
        </button>
        <Button type="button" variant="ghost" size="icon-sm" onClick={onClose} aria-label="Close">
          <X />
        </Button>
      </div>
      <Textarea
        rows={3}
        className="max-h-[40dvh] overflow-y-auto"
        placeholder="Meaning"
        value={shownMeaning}
        onChange={(e) => setMeaning(e.target.value)}
        disabled={saving}
      />
      <div className="flex gap-2">
        <NativeSelect
          aria-label="Level"
          className="min-w-0 flex-1"
          value={level}
          onChange={(e) => setLevel(e.target.value as LearningLevel)}
          disabled={saving}
        >
          {EDITABLE_LEVELS.map((l) => (
            <NativeSelectOption key={l} value={l}>
              {LEVEL_LABELS[l]}
            </NativeSelectOption>
          ))}
        </NativeSelect>
        <Button type="button" variant="outline" disabled={saving} onClick={() => void save(nextLevel(level))}>
          <ArrowUp />
          Better
        </Button>
        <Button type="submit" disabled={saving}>
          Save
        </Button>
      </div>
    </form>
  );
}

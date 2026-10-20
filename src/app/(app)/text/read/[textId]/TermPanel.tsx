'use client';

import { useState } from 'react';
import { importanceColor } from '@/lib/importance';
import { LearningLevel, LEVEL_LABELS, nextLevel, type DictionaryLookup, type TextTermView } from '@/lib/types';
import { ArrowUp, ChevronLeft, ChevronRight, Copy, Link, Unlink, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { NativeSelect, NativeSelectOption } from '@/components/ui/native-select';
import { Textarea } from '@/components/ui/textarea';
import DictionaryHint, { suggestionOf } from './DictionaryHint';
import { panelMeaning } from './termEditing';

type Props = {
  /** A word, a saved phrase, or (id null) a phrase made of the selected words that saving creates. */
  term: TextTermView;
  lookup: DictionaryLookup | undefined;
  /** For selected words not joined here: the scope saving starts with, or loading while it is not known yet. */
  phraseScope?: 'here' | 'everywhere' | 'loading';
  /**
   * everywhere: a new phrase joins in all texts, not only here.
   * Resolves to null once saved, or to the error to show while the panel stays open.
   */
  onSave: (level: LearningLevel, meaning: string, everywhere: boolean) => Promise<string | null>;
  /** Add the word before or after to the selection; missing when punctuation is in the way. */
  onJoinBefore?: () => void;
  onJoinAfter?: () => void;
  /** For a saved phrase: split it here or in all texts, or join it in all texts. Resolves like onSave. */
  onPhraseAction?: (action: PhraseAction) => Promise<string | null>;
  onClose: () => void;
};

export type PhraseAction = 'split-here' | 'split-everywhere' | 'join-everywhere';

const EDITABLE_LEVELS = Object.values(LearningLevel).filter((l) => l !== LearningLevel.Skipped);

export default function TermPanel({
  term,
  phraseScope,
  lookup,
  onSave,
  onJoinBefore,
  onJoinAfter,
  onPhraseAction,
  onClose,
}: Props) {
  // Null until the user types in the box.
  const [edited, setEdited] = useState<string | null>(null);
  const [level, setLevel] = useState(term.learningLevel);
  const [everywhere, setEverywhere] = useState(phraseScope !== 'here');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const shownMeaning = panelMeaning(edited, term.meaning, suggestionOf(lookup));
  const newPhrase = term.id == null;

  const busy = async (action: () => Promise<string | null>) => {
    setSaving(true);
    setError(null);
    setError(await action());
    setSaving(false);
  };
  const save = (nextLevelValue: LearningLevel) => busy(() => onSave(nextLevelValue, shownMeaning, everywhere));

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
          className="flex min-w-0 items-center gap-1.5 text-xl font-bold hover:opacity-80 [&>svg]:size-3.5 [&>svg]:shrink-0 [&>svg]:text-muted-foreground"
          title="Copy"
          style={{ color: term.count ? importanceColor(term.count) : undefined }}
          onClick={() => void navigator.clipboard?.writeText(term.content)}
        >
          <span className="truncate">{term.id == null ? term.content : `${term.content} (${term.count ?? '-'})`}</span>
          <Copy />
        </button>
        <div className="flex flex-none items-center">
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            disabled={!onJoinBefore || saving}
            onClick={onJoinBefore}
            aria-label="Join the previous word"
            title="Join the previous word"
          >
            <ChevronLeft />
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            disabled={!onJoinAfter || saving}
            onClick={onJoinAfter}
            aria-label="Join the next word"
            title="Join the next word (or shift-click it)"
          >
            <ChevronRight />
          </Button>
          {onPhraseAction && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button type="button" variant="ghost" size="icon-sm" disabled={saving} aria-label="Phrase" title="Split or join">
                  <Unlink />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onSelect={() => void busy(() => onPhraseAction('split-here'))}>
                  <Unlink />
                  Split here only
                </DropdownMenuItem>
                <DropdownMenuItem onSelect={() => void busy(() => onPhraseAction('split-everywhere'))}>
                  <Unlink />
                  Split in all texts
                </DropdownMenuItem>
                {!term.autoJoin && (
                  <DropdownMenuItem onSelect={() => void busy(() => onPhraseAction('join-everywhere'))}>
                    <Link />
                    Join in all texts
                  </DropdownMenuItem>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          )}
          <Button type="button" variant="ghost" size="icon-sm" onClick={onClose} aria-label="Close">
            <X />
          </Button>
        </div>
      </div>

      <Textarea
        rows={3}
        className="max-h-[40dvh] overflow-y-auto"
        placeholder="Meaning"
        value={shownMeaning}
        onChange={(e) => setEdited(e.target.value)}
        disabled={saving}
      />
      {!term.meaning && edited === null && <DictionaryHint lookup={lookup} />}
      {error && (
        <p className="text-sm text-destructive" role="alert">
          {error}
        </p>
      )}
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
        {newPhrase ? (
          <NativeSelect
            aria-label="Join where"
            className="w-32 flex-none"
            value={everywhere ? 'everywhere' : 'here'}
            onChange={(e) => setEverywhere(e.target.value === 'everywhere')}
            disabled={saving}
          >
            <NativeSelectOption value="everywhere">All texts</NativeSelectOption>
            <NativeSelectOption value="here">Here only</NativeSelectOption>
          </NativeSelect>
        ) : (
          <Button type="button" variant="outline" disabled={saving} onClick={() => void save(nextLevel(level))}>
            <ArrowUp />
            Better
          </Button>
        )}
        <Button type="submit" disabled={saving || phraseScope === 'loading'}>
          {newPhrase ? 'Save phrase' : 'Save'}
        </Button>
      </div>
    </form>
  );
}

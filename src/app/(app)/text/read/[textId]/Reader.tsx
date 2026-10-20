'use client';

import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import {
  joinEverywhere,
  loadPhrase,
  loadTermCounts,
  loadTextTerms,
  lookUpTerm,
  saveBookmark,
  savePhrase,
  saveTerm,
  unmakePhrase,
} from '@/lib/reader';
import { LearningLevel, type DictionaryLookup, type TermCounts, type TextReadInfo, type TextTermView } from '@/lib/types';
import Link from 'next/link';
import { languageName } from '@/lib/languages';
import { ArrowLeft, Bookmark, LoaderCircle } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import TermToken from './TermToken';
import TermPopover from './TermPopover';
import TermPanel, { type PhraseAction } from './TermPanel';
import { suggestionOf } from './DictionaryHint';
import TextStats from './TextStats';
import styles from './reader.module.css';

type Props = {
  text: TextReadInfo;
  initialCounts: Required<TermCounts>;
  speakCode: string | null;
  dictionaryLanguage: string | null;
  /** Between a phrase's words: a space for English, nothing for Chinese and Japanese. */
  phraseSeparator: string;
};

// Tokens loaded around the bookmark first, then this many more when scrolling near an edge.
const INITIAL_BEFORE = 500;
const INITIAL_AFTER = 1000;
const LOAD_MORE = 300;
const EDGE_PX = 200;

const NO_MEANING = new Set<LearningLevel>([LearningLevel.Skipped, LearningLevel.Ignored, LearningLevel.WellKnown]);

export type Hover = { index: number; rect: DOMRect };
/** Selected tokens, first to last, grown from the clicked one (anchor); more than one word makes a new phrase. */
type Selection = { from: number; to: number; anchor: number };
/** The saved phrase that selected words spell, looked up by `${from}-${to}`; term is null when there is none. */
type KnownPhrase = { range: string; term: Awaited<ReturnType<typeof loadPhrase>> };

// Mirrors src/server/languages/phrases.ts: words join across spaces with at most one line break.
const NO_PHRASE_GAP = /\S|\n[^]*\n/;

export default function Reader({ text, initialCounts, speakCode, dictionaryLanguage, phraseSeparator }: Props) {
  const lastIndex = text.termCount - 1;
  const start = Math.min(text.bookmark ?? 0, Math.max(lastIndex, 0));

  const [terms, setTerms] = useState<Map<number, TextTermView>>(new Map());
  const [range, setRange] = useState({ begin: Math.max(0, start - INITIAL_BEFORE), end: Math.min(lastIndex, start + INITIAL_AFTER) });
  const [bookmark, setBookmarkState] = useState<number | null>(text.bookmark);
  const [selection, setSelection] = useState<Selection | null>(null);
  const [knownPhrase, setKnownPhrase] = useState<KnownPhrase | null>(null);
  const [hover, setHover] = useState<Hover | null>(null);
  const [counts, setCounts] = useState(initialCounts);
  const [viewing, setViewing] = useState(start);
  // Dictionary lookups by lower-cased word or phrase; a found meaning is suggested until the user saves their own.
  const [lookups, setLookups] = useState<Map<string, DictionaryLookup>>(new Map());

  const panelRef = useRef<HTMLDivElement>(null);
  const loading = useRef(false);
  const initialScrollDone = useRef(false);
  const prependAnchor = useRef<{ height: number; top: number } | null>(null);

  const mergeTerms = useCallback((loaded: TextTermView[]) => {
    setTerms((prev) => {
      const next = new Map(prev);
      for (const t of loaded) next.set(t.index, t);
      return next;
    });
  }, []);

  // First load around the bookmark.
  useEffect(() => {
    if (text.termCount === 0) return;
    loading.current = true;
    loadTextTerms(text.id, range.begin, range.end)
      .then(mergeTerms)
      .finally(() => (loading.current = false));
    // Only on mount; later ranges are fetched as they grow.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // The element for a token index; one inside a phrase finds the phrase.
  const tokenElement = useCallback((index: number) => {
    for (let i = index; i >= Math.max(0, index - 100); i--) {
      const el = document.getElementById(`t${i}`);
      if (el) return el;
    }
    return null;
  }, []);

  // Once the first tokens render, bring the bookmark into view.
  useLayoutEffect(() => {
    if (initialScrollDone.current || terms.size === 0) return;
    initialScrollDone.current = true;
    tokenElement(start)?.scrollIntoView({ block: 'center' });
  }, [terms, start, tokenElement]);

  // Keep the reading position when tokens are added above it.
  useLayoutEffect(() => {
    const panel = panelRef.current;
    const anchor = prependAnchor.current;
    if (!panel || !anchor) return;
    panel.scrollTop = anchor.top + (panel.scrollHeight - anchor.height);
    prependAnchor.current = null;
  }, [terms]);

  const loadMore = useCallback(
    async (direction: 'before' | 'after') => {
      if (loading.current) return;
      const from = direction === 'before' ? Math.max(0, range.begin - LOAD_MORE) : range.end + 1;
      const to = direction === 'before' ? range.begin - 1 : Math.min(lastIndex, range.end + LOAD_MORE);
      if (from > to) return;
      loading.current = true;
      try {
        const loaded = await loadTextTerms(text.id, from, to);
        const panel = panelRef.current;
        if (direction === 'before' && panel) {
          prependAnchor.current = { height: panel.scrollHeight, top: panel.scrollTop };
        }
        mergeTerms(loaded);
        setRange((r) => (direction === 'before' ? { ...r, begin: from } : { ...r, end: to }));
      } finally {
        loading.current = false;
      }
    },
    [range, lastIndex, text.id, mergeTerms],
  );

  const onScroll = () => {
    const panel = panelRef.current;
    if (!panel) return;
    setHover(null);
    if (panel.scrollTop < EDGE_PX) void loadMore('before');
    if (panel.scrollHeight - panel.scrollTop - panel.clientHeight < EDGE_PX) void loadMore('after');
    // Progress follows the first token visible at the top of the panel.
    const box = panel.getBoundingClientRect();
    // The text column is centered, so probe from its left edge rather than the panel's.
    const left = panel.firstElementChild?.getBoundingClientRect().left ?? box.left;
    const el = document.elementFromPoint(left + 24, box.top + 24)?.closest<HTMLElement>('[data-index]');
    if (el) setViewing(Number(el.dataset.index));
  };

  const speak = useCallback(
    (word: string) => {
      if (!speakCode || typeof speechSynthesis === 'undefined') return;
      const utterance = new SpeechSynthesisUtterance(word);
      utterance.lang = speakCode;
      utterance.voice = speechSynthesis.getVoices().find((v) => v.lang.replace('_', '-') === speakCode.replace('_', '-')) ?? null;
      speechSynthesis.cancel();
      speechSynthesis.speak(utterance);
    },
    [speakCode],
  );

  const suggestMeaning = useCallback(
    (term: TextTermView) => {
      if (!dictionaryLanguage || term.meaning || NO_MEANING.has(term.learningLevel)) return;
      // Only a lookup that could not reach the dictionary is tried again.
      const key = lookupKey(term);
      const status = lookups.get(key)?.status;
      if (status && status !== 'unavailable') return;
      const settle = (lookup: DictionaryLookup) => setLookups((prev) => new Map(prev).set(key, lookup));
      settle({ status: 'loading' });
      lookUpTerm(term.content, text.languageCode, dictionaryLanguage).then(
        (meaning) =>
          settle(meaning ? { status: 'found', meaning } : meaning === null ? { status: 'missing' } : { status: 'unavailable' }),
        () => settle({ status: 'unavailable' }),
      );
    },
    [dictionaryLanguage, lookups, text.languageCode],
  );

  const markBookmark = useCallback(
    (index: number) => {
      setBookmarkState(index);
      void saveBookmark(text.id, index);
    },
    [text.id],
  );

  const updateTerm = useCallback(
    async (term: TextTermView, learningLevel: LearningLevel, meaning: string) => {
      if (term.id == null) return;
      const result = await saveTerm(term.id, learningLevel, meaning);
      if (!result.ok) {
        alert(result.error);
        return;
      }
      // Every occurrence of the term in the loaded part of the text changes together.
      setTerms((prev) => {
        const next = new Map(prev);
        for (const [i, t] of prev) {
          if (t.id === term.id) next.set(i, { ...t, learningLevel, meaning: NO_MEANING.has(learningLevel) ? null : meaning });
        }
        return next;
      });
      setCounts(await loadTermCounts(text.id));
    },
    [text.id],
  );

  // Making or splitting a phrase changes tokens across the text, so the loaded part is fetched again.
  const reload = useCallback(async () => {
    const loaded = await loadTextTerms(text.id, range.begin, range.end);
    setTerms(new Map(loaded.map((t) => [t.index, t])));
    setCounts(await loadTermCounts(text.id));
  }, [text.id, range]);

  /** The next word from `index` in direction `step` that a phrase could join, or null past punctuation or a paragraph. */
  const joinableWord = useCallback(
    (index: number, step: 1 | -1) => {
      let gap = '';
      for (let i = index + step; i >= range.begin && i <= range.end; i += step) {
        const t = terms.get(i);
        if (!t) continue; // inside a phrase
        if (t.id != null) return i;
        gap += t.content;
        if (!phraseSeparator || NO_PHRASE_GAP.test(gap)) return null;
      }
      return null;
    },
    [terms, range, phraseSeparator],
  );

  const select = useCallback(
    (next: Selection) => {
      setSelection(next);
      const tokens = tokensIn(terms, next);
      if (tokens.length === 1) {
        speak(tokens[0].content);
        suggestMeaning(tokens[0]);
      } else {
        const phrase = phraseView(tokens, next.from, null);
        speak(phrase.content);
        suggestMeaning(phrase);
        // Words joined elsewhere, or split here, are already a phrase term: show its level and meaning.
        const range = `${next.from}-${next.to}`;
        void loadPhrase(text.id, next.from, next.to).then((term) => setKnownPhrase({ range, term }));
      }
    },
    [terms, speak, suggestMeaning, text.id],
  );

  const onTermClick = useCallback(
    (term: TextTermView, extend: boolean) => {
      setHover(null);
      // Shift-click grows the selection into a phrase, when only spaces lie between.
      if (extend && selection) {
        const from = Math.min(selection.from, term.index);
        const to = Math.max(selection.to, term.index);
        let i: number | null = from;
        while (i != null && i < to) i = joinableWord(i, 1);
        if (i === to) {
          select({ from, to, anchor: selection.anchor });
          return;
        }
      }
      markBookmark(term.index);
      select({ from: term.index, to: term.index, anchor: term.index });
    },
    [selection, joinableWord, markBookmark, select],
  );

  // The popover stays up briefly after the pointer leaves, so it can be reached.
  const hideTimer = useRef<ReturnType<typeof setTimeout>>(undefined);
  const cancelHide = useCallback(() => clearTimeout(hideTimer.current), []);
  const scheduleHide = useCallback(() => {
    clearTimeout(hideTimer.current);
    hideTimer.current = setTimeout(() => setHover(null), 300);
  }, []);

  const onTermHover = useCallback(
    (term: TextTermView, rect: DOMRect | null) => {
      if (!rect) {
        scheduleHide();
        return;
      }
      cancelHide();
      setHover({ index: term.index, rect });
      speak(term.content);
      suggestMeaning(term);
    },
    [speak, suggestMeaning, cancelHide, scheduleHide],
  );

  // A bookmark on a word inside a phrase shows on the phrase.
  let bookmarkedIndex = bookmark;
  while (bookmarkedIndex != null && bookmarkedIndex > range.begin && !terms.has(bookmarkedIndex)) bookmarkedIndex--;

  const tokens = [];
  for (let i = range.begin; i <= range.end; i++) {
    const term = terms.get(i);
    if (term) {
      tokens.push(
        <TermToken
          key={i}
          term={term}
          bookmarked={bookmarkedIndex === i}
          selected={selection != null && i >= selection.from && i <= selection.to}
          onClick={onTermClick}
          onHover={onTermHover}
        />,
      );
    }
  }

  const selectedTokens = selection ? tokensIn(terms, selection) : [];
  const selectedWords = selectedTokens.filter((t) => t.id != null);
  const selectedTerm: PhraseView | undefined =
    selection == null || selectedWords.length === 0
      ? undefined
      : selectedWords.length === 1
        ? selectedWords[0]
        : phraseView(
            selectedTokens,
            selection.from,
            knownPhrase?.range === `${selection.from}-${selection.to}` ? knownPhrase : undefined,
          );
  // New words join everywhere by default; a saved phrase's other occurrences were left apart on purpose.
  const phraseScope =
    selectedTerm?.id !== null ? undefined : !selectedTerm.lookedUp ? 'loading' : selectedTerm.phraseId ? 'here' : 'everywhere';
  const joinBefore = selection && joinableWord(selection.from, -1);
  const joinAfter = selection && joinableWord(selection.to, 1);
  const hoverTerm = hover == null ? undefined : terms.get(hover.index);
  const percent = text.termCount ? Math.round(((bookmark ?? 0) / text.termCount) * 10000) / 100 : 0;

  return (
    <div className={`${styles.reader} flex h-[calc(100dvh-3.5rem)] flex-col`}>
      <title>{text.title}</title>
      <header className="flex-none border-b bg-card">
        <div className="mx-auto flex max-w-3xl flex-col gap-2.5 px-4 py-3">
          <div className="flex min-w-0 items-center gap-2">
            <Button asChild variant="ghost" size="icon-sm" className="-ml-2">
              <Link href="/text" aria-label="Back to texts" title="Back to texts">
                <ArrowLeft />
              </Link>
            </Button>
            <h1 className="truncate text-lg font-semibold">{text.title}</h1>
            <Badge variant="secondary">{languageName(text.languageCode)}</Badge>
          </div>
          <TextStats counts={counts} termCount={text.termCount} />
        </div>
      </header>
      {text.processedTermCount < text.termCount || text.termCount === 0 ? (
        <p className="flex-none bg-amber-500/10 px-4 py-2 text-center text-sm text-amber-700 dark:text-amber-400">
          This text is still being split into words. Reload in a moment to see all of it.
        </p>
      ) : null}
      <div className="flex min-h-0 flex-1">
        <div ref={panelRef} className="flex-1 overflow-y-auto [overflow-anchor:none]" onScroll={onScroll} lang={text.languageCode}>
          <div className={styles.page}>
            {range.begin > 0 && <p className="mb-4 text-center text-sm leading-normal text-muted-foreground">Scroll up for more</p>}
            {tokens}
            {terms.size === 0 && text.termCount > 0 && (
              <p className="flex items-center gap-2 text-sm leading-normal text-muted-foreground">
                <LoaderCircle className="size-4 animate-spin" />
                Loading…
              </p>
            )}
          </div>
        </div>
        <div className="my-3 mr-2 ml-1 w-1 flex-none overflow-hidden rounded-full bg-muted" aria-hidden>
          <div className="rounded-full bg-primary" style={{ height: `${text.termCount ? (viewing * 100) / text.termCount : 0}%` }} />
        </div>
      </div>
      {selection && selectedTerm && (
        <TermPanel
          // Joining words keeps the panel, and what was typed in it; a saved phrase's level and meaning replace it.
          key={`${selection.anchor}-${selectedTerm.id ?? `phrase-${selectedTerm.phraseId ?? ''}`}`}
          term={selectedTerm}
          phraseScope={phraseScope}
          lookup={lookups.get(lookupKey(selectedTerm))}
          onSave={async (level, meaning, everywhere) => {
            if (selectedTerm.id != null) {
              await updateTerm(selectedTerm, level, meaning);
            } else {
              const result = await savePhrase(text.id, selection.from, selection.to, level, meaning, everywhere);
              if (!result.ok) return alert(result.error);
              await reload();
            }
            setSelection(null);
          }}
          onJoinBefore={joinBefore == null ? undefined : () => select({ ...selection, from: joinBefore })}
          onJoinAfter={joinAfter == null ? undefined : () => select({ ...selection, to: joinAfter })}
          onPhraseAction={
            selectedTerm.phrase && selectedTerm.id != null
              ? async (action: PhraseAction) => {
                  const id = selectedTerm.id!;
                  const result =
                    action === 'join-everywhere'
                      ? await joinEverywhere(id)
                      : await unmakePhrase(id, action === 'split-here' ? { textId: text.id, index: selectedTerm.index } : null);
                  if (!result.ok) return alert(result.error);
                  await reload();
                  setSelection(null);
                }
              : undefined
          }
          onClose={() => setSelection(null)}
        />
      )}
      {hover && hoverTerm && hoverTerm.id != null && (
        <TermPopover
          term={hoverTerm}
          rect={hover.rect}
          lookup={lookups.get(lookupKey(hoverTerm))}
          onKeep={cancelHide}
          onLeave={scheduleHide}
          onChangeLevel={async (level) => {
            markBookmark(hoverTerm.index);
            await updateTerm(hoverTerm, level, hoverTerm.meaning ?? suggestionOf(lookups.get(lookupKey(hoverTerm))) ?? '');
          }}
        />
      )}
      {!selection && bookmark != null && (
        <Button
          size="lg"
          className="fixed right-6 bottom-6 z-20 rounded-full bg-bookmark px-5 text-white shadow-lg tabular-nums hover:bg-bookmark/90"
          title="Go to bookmark"
          onClick={() => tokenElement(bookmark)?.scrollIntoView({ block: 'center', behavior: 'smooth' })}
        >
          <Bookmark />
          {`${percent}%`}
        </Button>
      )}
    </div>
  );
}

function tokensIn(terms: Map<number, TextTermView>, { from, to }: Selection) {
  const tokens = [];
  for (let i = from; i <= to; i++) {
    const t = terms.get(i);
    if (t) tokens.push(t);
  }
  return tokens;
}

function lookupKey(term: TextTermView) {
  return term.content.toLowerCase();
}

/** Selected words not joined here yet; phraseId is set when they spell a saved phrase, lookedUp once that is known. */
type PhraseView = TextTermView & { phraseId?: number; lookedUp?: boolean };

/** A phrase not saved yet, from the selected tokens: spaces and line breaks between its words read as one space. */
function phraseView(tokens: TextTermView[], index: number, known: KnownPhrase | null | undefined): PhraseView {
  return {
    id: null,
    content: tokens
      .map((t) => t.content)
      .join('')
      .replace(/\s+/g, ' '),
    learningLevel: known?.term?.learningLevel ?? LearningLevel.Unknown,
    index,
    meaning: known?.term?.meaning || null,
    count: null,
    phrase: true,
    autoJoin: known?.term?.autoJoin ?? false,
    phraseId: known?.term?.id,
    lookedUp: known !== undefined,
  };
}

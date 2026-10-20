'use client';

import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import { loadTermCounts, loadTextProgress, loadTextTerms, lookUpTerm, saveBookmark, saveTerm } from '@/lib/reader';
import { advanceProcessing } from '@/lib/texts';
import {
  LearningLevel,
  type DictionaryLookup,
  type TermCounts,
  type TextProgress,
  type TextReadInfo,
  type TextTermView,
} from '@/lib/types';
import Link from 'next/link';
import { languageName } from '@/lib/languages';
import { ArrowLeft, Bookmark, CircleAlert, LoaderCircle, RotateCw } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import TermToken from './TermToken';
import TermPopover from './TermPopover';
import TermPanel from './TermPanel';
import { suggestionOf } from './DictionaryHint';
import { panelMeaning, saveError } from './termEditing';
import TextStats from './TextStats';
import styles from './reader.module.css';

type Props = {
  text: TextReadInfo;
  initialCounts: Required<TermCounts>;
  speakCode: string | null;
  dictionaryLanguage: string | null;
};

// Tokens loaded around the bookmark first, then this many more when scrolling near an edge.
const INITIAL_BEFORE = 500;
const INITIAL_AFTER = 1000;
const LOAD_MORE = 300;
const EDGE_PX = 200;
// While the text is still being split, how often to check for newly split tokens.
const POLL_MS = 1000;

const NO_MEANING = new Set<LearningLevel>([LearningLevel.Skipped, LearningLevel.Ignored, LearningLevel.WellKnown]);

export type Hover = { index: number; rect: DOMRect };

type Range = { begin: number; end: number };

/** Where reading starts: the bookmark, within the text. */
function startOf(termCount: number, bookmark: number | null) {
  return Math.min(bookmark ?? 0, Math.max(termCount - 1, 0));
}

function initialRange(termCount: number, bookmark: number | null): Range {
  const start = startOf(termCount, bookmark);
  return { begin: Math.max(0, start - INITIAL_BEFORE), end: Math.min(termCount - 1, start + INITIAL_AFTER) };
}

const isProcessing = (p: TextProgress) => !p.processingError && (p.termCount === 0 || p.processedTermCount < p.termCount);

/** A failed load, shown where its tokens would be; Retry runs it again. */
type LoadError = { where: 'top' | 'bottom'; retry: () => void };

function LoadErrorNotice({ error }: { error: LoadError }) {
  return (
    <div role="alert" className="flex flex-wrap items-center justify-center gap-x-3 gap-y-2 py-3 text-sm leading-normal whitespace-normal">
      <span className="flex items-center gap-2 text-destructive">
        <CircleAlert className="size-4 flex-none" />
        Couldn&apos;t load this part of the text.
      </span>
      <Button variant="outline" size="sm" onClick={error.retry}>
        <RotateCw />
        Retry
      </Button>
    </div>
  );
}

export default function Reader({ text, initialCounts, speakCode, dictionaryLanguage }: Props) {
  // Splitting may still be going on; tokens below processedTermCount can be read.
  const [progress, setProgress] = useState<TextProgress>({
    termCount: text.termCount,
    processedTermCount: text.processedTermCount,
    processingError: text.processingError,
  });
  const processing = isProcessing(progress);
  const lastIndex = progress.termCount - 1;

  const [terms, setTerms] = useState<Map<number, TextTermView>>(new Map());
  const [range, setRange] = useState(() => initialRange(text.termCount, text.bookmark));
  const [bookmark, setBookmarkState] = useState<number | null>(text.bookmark);
  const start = startOf(progress.termCount, bookmark);
  const [loadError, setLoadError] = useState<LoadError | null>(null);
  const [countsError, setCountsError] = useState(false);
  const [selected, setSelected] = useState<number | null>(null);
  const [hover, setHover] = useState<Hover | null>(null);
  const [counts, setCounts] = useState(initialCounts);
  const [viewing, setViewing] = useState(start);
  // Dictionary lookups by term id; a found meaning is suggested until the user saves their own.
  const [lookups, setLookups] = useState<Map<number, DictionaryLookup>>(new Map());

  const panelRef = useRef<HTMLDivElement>(null);
  const loading = useRef(false);
  const initialScrollDone = useRef(false);
  const prependAnchor = useRef<{ height: number; top: number } | null>(null);
  // The latest range, for the processing poll, which shouldn't restart on every scroll load.
  const rangeRef = useRef(range);
  useEffect(() => {
    rangeRef.current = range;
  }, [range]);

  const mergeTerms = useCallback((loaded: TextTermView[]) => {
    setTerms((prev) => {
      const next = new Map(prev);
      for (const t of loaded) next.set(t.index, t);
      return next;
    });
  }, []);

  const refreshCounts = useCallback(() => {
    loadTermCounts(text.id).then(
      (c) => {
        setCounts(c);
        setCountsError(false);
      },
      () => setCountsError(true),
    );
  }, [text.id]);

  // First load around the bookmark, of the part that has been split so far; Retry runs it again.
  const [initialAttempt, setInitialAttempt] = useState(0);
  useEffect(() => {
    const to = Math.min(range.end, progress.processedTermCount - 1);
    if (range.begin > to) return;
    loading.current = true;
    loadTextTerms(text.id, range.begin, to)
      .then(mergeTerms, () =>
        setLoadError({
          where: 'bottom',
          retry: () => {
            setLoadError(null);
            setInitialAttempt((n) => n + 1);
          },
        }),
      )
      .finally(() => (loading.current = false));
    // Later ranges are fetched as they grow, so this only runs on mount and on Retry.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialAttempt]);

  // Once the first tokens render, bring the bookmark into view.
  useLayoutEffect(() => {
    if (initialScrollDone.current || terms.size === 0) return;
    initialScrollDone.current = true;
    document.getElementById(`t${start}`)?.scrollIntoView({ block: 'center' });
  }, [terms, start]);

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
      // Past the split part the processing poll fills tokens in as they are ready.
      if (from > to || from >= progress.processedTermCount) return;
      loading.current = true;
      try {
        const loaded = await loadTextTerms(text.id, from, to);
        const panel = panelRef.current;
        if (direction === 'before' && panel) {
          prependAnchor.current = { height: panel.scrollHeight, top: panel.scrollTop };
        }
        mergeTerms(loaded);
        setRange((r) => (direction === 'before' ? { ...r, begin: from } : { ...r, end: to }));
      } catch {
        // Clearing the error loads again from the edge the reader is at.
        setLoadError({ where: direction === 'before' ? 'top' : 'bottom', retry: () => setLoadError(null) });
      } finally {
        loading.current = false;
      }
    },
    [range, lastIndex, progress.processedTermCount, text.id, mergeTerms],
  );

  const loadNearEdges = useCallback(() => {
    const panel = panelRef.current;
    if (!panel || loadError) return;
    if (panel.scrollTop < EDGE_PX) void loadMore('before');
    if (panel.scrollHeight - panel.scrollTop - panel.clientHeight < EDGE_PX) void loadMore('after');
  }, [loadMore, loadError]);

  // Keep loading while the reader sits at an edge, e.g. at the end of what has been split so far.
  useEffect(() => {
    if (initialScrollDone.current) loadNearEdges();
  }, [terms, loadNearEdges]);

  // While the text is being split, pick up the newly split tokens of the loaded range.
  useEffect(() => {
    if (!processing || loadError) return;
    let stopped = false;
    const timer = setTimeout(async () => {
      try {
        // Where no background loop runs (serverless), this does the splitting.
        await advanceProcessing().catch(() => undefined);
        const next = await loadTextProgress(text.id);
        if (stopped) return;
        let r = rangeRef.current;
        let from = progress.processedTermCount;
        if (next.termCount !== progress.termCount) {
          // Split (again) from scratch: start over around the bookmark.
          r = initialRange(next.termCount, bookmark);
          from = 0;
          initialScrollDone.current = false;
          setTerms(new Map());
          setRange(r);
        }
        const to = Math.min(r.end, next.processedTermCount - 1);
        if (Math.max(r.begin, from) <= to) {
          const loaded = await loadTextTerms(text.id, Math.max(r.begin, from), to);
          if (stopped) return;
          mergeTerms(loaded);
        }
        setProgress(next);
        if (next.processedTermCount !== progress.processedTermCount) refreshCounts();
      } catch {
        if (stopped) return;
        setLoadError({ where: 'bottom', retry: () => setLoadError(null) });
      }
    }, POLL_MS);
    return () => {
      stopped = true;
      clearTimeout(timer);
    };
  }, [processing, progress, loadError, bookmark, text.id, mergeTerms, refreshCounts]);

  const onScroll = () => {
    const panel = panelRef.current;
    if (!panel) return;
    setHover(null);
    loadNearEdges();
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
      if (!dictionaryLanguage || term.id == null || term.meaning || NO_MEANING.has(term.learningLevel)) return;
      // Only a lookup that could not reach the dictionary is tried again.
      const status = lookups.get(term.id)?.status;
      if (status && status !== 'unavailable') return;
      const id = term.id;
      const settle = (lookup: DictionaryLookup) => setLookups((prev) => new Map(prev).set(id, lookup));
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
      // The bookmark only marks the reading position; the next click saves it again.
      saveBookmark(text.id, index).catch(() => {});
    },
    [text.id],
  );

  const updateTerm = useCallback(
    /** Resolves to null once saved, or to the error to show; never rejects. */
    async (term: TextTermView, learningLevel: LearningLevel, meaning: string) => {
      if (term.id == null) return null;
      const error = await saveError(saveTerm(term.id, learningLevel, meaning));
      if (error) return error;
      // Every occurrence of the term in the loaded part of the text changes together.
      setTerms((prev) => {
        const next = new Map(prev);
        for (const [i, t] of prev) {
          if (t.id === term.id) next.set(i, { ...t, learningLevel, meaning });
        }
        return next;
      });
      // The term is saved either way; failing to refresh the counts only shows a notice.
      refreshCounts();
      return null;
    },
    [refreshCounts],
  );

  const onTermClick = useCallback(
    (term: TextTermView) => {
      setHover(null);
      setSelected(term.index);
      markBookmark(term.index);
      speak(term.content);
      suggestMeaning(term);
    },
    [markBookmark, speak, suggestMeaning],
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

  const tokens = [];
  for (let i = range.begin; i <= range.end; i++) {
    const term = terms.get(i);
    if (term) {
      tokens.push(
        <TermToken key={i} term={term} bookmarked={bookmark === i} selected={selected === i} onClick={onTermClick} onHover={onTermHover} />,
      );
    }
  }

  const selectedTerm = selected == null ? undefined : terms.get(selected);
  const hoverTerm = hover == null ? undefined : terms.get(hover.index);
  const percent = progress.termCount ? Math.round(((bookmark ?? 0) / progress.termCount) * 10000) / 100 : 0;
  const splitPercent = progress.termCount ? Math.floor((progress.processedTermCount * 100) / progress.termCount) : 0;

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
          {/* Counts cover the tokens split so far. */}
          <TextStats counts={counts} termCount={progress.processedTermCount} />
          {countsError && (
            <p role="alert" className="flex flex-wrap items-center gap-x-2 text-xs text-destructive">
              <CircleAlert className="size-3.5" />
              Couldn&apos;t update the word counts.
              <Button variant="link" size="sm" className="h-auto p-0 text-xs" onClick={refreshCounts}>
                Retry
              </Button>
            </p>
          )}
        </div>
      </header>
      {progress.processingError ? (
        <p className="flex-none bg-destructive/10 px-4 py-2 text-center text-sm break-words text-destructive" role="alert">
          {`Splitting this text into words failed: ${progress.processingError} — `}
          <Link href={`/text/${text.id}/edit`} className="font-medium underline underline-offset-2">
            Edit the text to retry
          </Link>
        </p>
      ) : processing ? (
        <p className="flex flex-none items-center justify-center gap-2 bg-amber-500/10 px-4 py-2 text-center text-sm text-amber-700 dark:text-amber-400">
          <LoaderCircle className="size-4 flex-none animate-spin" />
          {`This text is still being split into words${splitPercent ? ` (${splitPercent}%)` : ''}. The rest appears here as it's ready.`}
        </p>
      ) : null}
      <div className="flex min-h-0 flex-1">
        <div ref={panelRef} className="flex-1 overflow-y-auto [overflow-anchor:none]" onScroll={onScroll} lang={text.languageCode}>
          <div className={styles.page}>
            {loadError?.where === 'top' && <LoadErrorNotice error={loadError} />}
            {range.begin > 0 && !loadError && (
              <p className="mb-4 text-center text-sm leading-normal text-muted-foreground">Scroll up for more</p>
            )}
            {tokens}
            {loadError?.where === 'bottom' && <LoadErrorNotice error={loadError} />}
            {terms.size === 0 && !loadError && (processing || progress.termCount > 0) && (
              <p className="flex items-center gap-2 text-sm leading-normal text-muted-foreground">
                <LoaderCircle className="size-4 animate-spin" />
                Loading…
              </p>
            )}
          </div>
        </div>
        <div className="my-3 mr-2 ml-1 w-1 flex-none overflow-hidden rounded-full bg-muted" aria-hidden>
          <div className="rounded-full bg-primary" style={{ height: `${progress.termCount ? (viewing * 100) / progress.termCount : 0}%` }} />
        </div>
      </div>
      {selectedTerm && selectedTerm.id != null && (
        <TermPanel
          key={selectedTerm.index}
          term={selectedTerm}
          lookup={lookups.get(selectedTerm.id)}
          onSave={async (level, meaning) => {
            const error = await updateTerm(selectedTerm, level, meaning);
            if (!error) setSelected(null);
            return error;
          }}
          onClose={() => setSelected(null)}
        />
      )}
      {hover && hoverTerm && hoverTerm.id != null && (
        <TermPopover
          key={hover.index}
          term={hoverTerm}
          rect={hover.rect}
          lookup={lookups.get(hoverTerm.id)}
          onKeep={cancelHide}
          onLeave={scheduleHide}
          onChangeLevel={(level) => {
            markBookmark(hoverTerm.index);
            return updateTerm(hoverTerm, level, panelMeaning(null, hoverTerm.meaning, suggestionOf(lookups.get(hoverTerm.id!))));
          }}
        />
      )}
      {!selectedTerm && bookmark != null && (
        <Button
          size="lg"
          className="fixed right-6 bottom-6 z-20 rounded-full bg-bookmark px-5 text-white shadow-lg tabular-nums hover:bg-bookmark/90"
          title="Go to bookmark"
          onClick={() => document.getElementById(`t${bookmark}`)?.scrollIntoView({ block: 'center', behavior: 'smooth' })}
        >
          <Bookmark />
          {`${percent}%`}
        </Button>
      )}
    </div>
  );
}

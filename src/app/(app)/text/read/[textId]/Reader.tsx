'use client';

import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import { loadTermCounts, loadTextTerms, lookUpTerm, saveBookmark, saveTerm } from '@/lib/reader';
import { LearningLevel, type DictionaryLookup, type TermCounts, type TextReadInfo, type TextTermView } from '@/lib/types';
import Link from 'next/link';
import { languageName } from '@/lib/languages';
import { ArrowLeft, Bookmark, LoaderCircle } from 'lucide-react';
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

const NO_MEANING = new Set<LearningLevel>([LearningLevel.Skipped, LearningLevel.Ignored, LearningLevel.WellKnown]);

export type Hover = { index: number; rect: DOMRect };

export default function Reader({ text, initialCounts, speakCode, dictionaryLanguage }: Props) {
  const lastIndex = text.termCount - 1;
  const start = Math.min(text.bookmark ?? 0, Math.max(lastIndex, 0));

  const [terms, setTerms] = useState<Map<number, TextTermView>>(new Map());
  const [range, setRange] = useState({ begin: Math.max(0, start - INITIAL_BEFORE), end: Math.min(lastIndex, start + INITIAL_AFTER) });
  const [bookmark, setBookmarkState] = useState<number | null>(text.bookmark);
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
      // The term is saved either way; stale counts catch up on the next save.
      loadTermCounts(text.id).then(setCounts, () => {});
      return null;
    },
    [text.id],
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
  const percent = text.termCount ? Math.round(((bookmark ?? 0) / text.termCount) * 10000) / 100 : 0;

  return (
    // Pinned under the app header instead of restyling <main>: cacheComponents keeps visited pages mounted in
    // <main>, so a parent selector would leak to them. z-40 lets the popover overlap the header (z-30).
    <div className="fixed inset-x-0 top-14 bottom-0 z-40 flex flex-col bg-background">
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

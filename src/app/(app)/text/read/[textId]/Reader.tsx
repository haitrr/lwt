'use client';

import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import { loadTermCounts, loadTextTerms, lookUpTerm, saveBookmark, saveTerm } from '@/lib/reader';
import { LearningLevel, type TermCounts, type TextReadInfo, type TextTermView } from '@/lib/types';
import Link from 'next/link';
import { languageName } from '@/lib/languages';
import { ArrowLeftIcon, BookmarkIcon } from '@/components/icons';
import TermToken from './TermToken';
import TermPopover from './TermPopover';
import TermPanel from './TermPanel';
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
  // Dictionary suggestions by term id, shown until the user saves their own meaning.
  const [suggestions, setSuggestions] = useState<Map<number, string>>(new Map());

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
      if (suggestions.has(term.id)) return;
      const id = term.id;
      void lookUpTerm(term.content, text.languageCode, dictionaryLanguage).then((meaning) => {
        if (meaning) setSuggestions((prev) => new Map(prev).set(id, meaning));
      });
    },
    [dictionaryLanguage, suggestions, text.languageCode],
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
    <div className={styles.reader}>
      <title>{text.title}</title>
      <header className={styles.top}>
        <div className={styles.titleRow}>
          <Link href="/text" className={`button ghost small ${styles.back}`} aria-label="Back to texts" title="Back to texts">
            <ArrowLeftIcon />
          </Link>
          <h1 className={styles.title}>{text.title}</h1>
          <span className="badge">{languageName(text.languageCode)}</span>
        </div>
        <TextStats counts={counts} termCount={text.termCount} />
      </header>
      {text.processedTermCount < text.termCount || text.termCount === 0 ? (
        <p className={styles.notice}>This text is still being split into words. Reload in a moment to see all of it.</p>
      ) : null}
      <div className={styles.body}>
        <div ref={panelRef} className={styles.content} onScroll={onScroll} lang={text.languageCode}>
          <div className={styles.page}>
            {range.begin > 0 && <p className={styles.more}>Scroll up for more</p>}
            {tokens}
            {terms.size === 0 && text.termCount > 0 && <p className="loading">Loading…</p>}
          </div>
        </div>
        <div className={styles.progress} aria-hidden>
          <div style={{ height: `${text.termCount ? (viewing * 100) / text.termCount : 0}%` }} />
        </div>
      </div>
      {selectedTerm && selectedTerm.id != null && (
        <TermPanel
          key={selectedTerm.index}
          term={selectedTerm}
          suggestion={suggestions.get(selectedTerm.id) ?? null}
          onSave={async (level, meaning) => {
            await updateTerm(selectedTerm, level, meaning);
            setSelected(null);
          }}
          onClose={() => setSelected(null)}
        />
      )}
      {hover && hoverTerm && hoverTerm.id != null && (
        <TermPopover
          term={hoverTerm}
          rect={hover.rect}
          suggestion={suggestions.get(hoverTerm.id) ?? null}
          onKeep={cancelHide}
          onLeave={scheduleHide}
          onChangeLevel={async (level) => {
            markBookmark(hoverTerm.index);
            await updateTerm(hoverTerm, level, hoverTerm.meaning ?? suggestions.get(hoverTerm.id!) ?? '');
          }}
        />
      )}
      {!selectedTerm && bookmark != null && (
        <button
          type="button"
          className={styles.bookmarkButton}
          title="Go to bookmark"
          onClick={() => document.getElementById(`t${bookmark}`)?.scrollIntoView({ block: 'center', behavior: 'smooth' })}
        >
          <BookmarkIcon />
          {`${percent}%`}
        </button>
      )}
    </div>
  );
}

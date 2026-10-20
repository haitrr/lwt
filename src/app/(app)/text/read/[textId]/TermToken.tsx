'use client';

import { memo, useEffect, useRef } from 'react';
import { importanceColor } from '@/lib/importance';
import { isLearningTerm, LearningLevel, type TextTermView } from '@/lib/types';
import styles from './reader.module.css';

type Props = {
  term: TextTermView;
  bookmarked: boolean;
  selected: boolean;
  onClick: (term: TextTermView) => void;
  onHover: (term: TextTermView, rect: DOMRect | null) => void;
};

const HOVER_DELAY_MS = 200;

function TermToken({ term, bookmarked, selected, onClick, onHover }: Props) {
  // Cancels the hover preview that is waiting to open, if any.
  const cancelPending = useRef<() => void>(undefined);
  const cancelHover = () => cancelPending.current?.();
  useEffect(() => () => cancelPending.current?.(), []);
  const id = `t${term.index}`;

  if (term.learningLevel === LearningLevel.Skipped) {
    return (
      <span id={id} data-index={term.index} className={styles.skipped}>
        {term.content}
      </span>
    );
  }

  const learning = isLearningTerm(term.learningLevel);
  const count = term.count ?? 0;
  // Hover previews are for mouse users and for terms that still have something to show.
  const previewable = learning && typeof window !== 'undefined' && window.matchMedia('(hover: hover)').matches;

  return (
    <span
      id={id}
      data-index={term.index}
      role="button"
      tabIndex={0}
      className={[styles.term, bookmarked && styles.bookmark, selected && styles.selected].filter(Boolean).join(' ')}
      data-level={term.learningLevel}
      style={learning && count ? { borderBottomColor: importanceColor(count) } : undefined}
      onClick={() => {
        cancelHover();
        onClick(term);
      }}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          cancelHover();
          onClick(term);
        }
      }}
      onMouseEnter={
        previewable
          ? (e) => {
              cancelHover();
              const target = e.currentTarget;
              const timer = setTimeout(() => {
                cancel();
                onHover(term, target.getBoundingClientRect());
              }, HOVER_DELAY_MS);
              // Scrolling moves the token away from the pointer, so the preview would open in the wrong place.
              const cancel = () => {
                clearTimeout(timer);
                window.removeEventListener('scroll', cancel, true);
                cancelPending.current = undefined;
              };
              window.addEventListener('scroll', cancel, true);
              cancelPending.current = cancel;
            }
          : undefined
      }
      onMouseLeave={
        previewable
          ? () => {
              cancelHover();
              onHover(term, null);
            }
          : undefined
      }
    >
      {term.content}
      {learning && count > 0 && <sup>{count}</sup>}
    </span>
  );
}

export default memo(TermToken);

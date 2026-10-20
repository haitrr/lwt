'use client';

import { memo, useRef } from 'react';
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
  const timer = useRef<ReturnType<typeof setTimeout>>(undefined);
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
      onClick={() => onClick(term)}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onClick(term);
        }
      }}
      onMouseEnter={
        previewable
          ? (e) => {
              const target = e.currentTarget;
              timer.current = setTimeout(() => onHover(term, target.getBoundingClientRect()), HOVER_DELAY_MS);
            }
          : undefined
      }
      onMouseLeave={
        previewable
          ? () => {
              clearTimeout(timer.current);
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

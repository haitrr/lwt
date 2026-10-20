'use client';

import { importanceColor } from '@/lib/importance';
import { nextLevel, previousLevel, LEVEL_LABELS, type LearningLevel, type TextTermView } from '@/lib/types';
import { ArrowDownIcon, ArrowUpIcon } from '@/components/icons';
import styles from './reader.module.css';

type Props = {
  term: TextTermView;
  rect: DOMRect;
  suggestion: string | null;
  onKeep: () => void;
  onLeave: () => void;
  onChangeLevel: (level: LearningLevel) => void;
};

export default function TermPopover({ term, rect, suggestion, onKeep, onLeave, onChangeLevel }: Props) {
  const meaning = term.meaning || suggestion;
  const above = rect.top > window.innerHeight / 2;
  return (
    <div
      className={styles.popover}
      role="dialog"
      style={{
        left: Math.min(rect.left, window.innerWidth - 320),
        ...(above ? { bottom: window.innerHeight - rect.top + 8 } : { top: rect.bottom + 8 }),
      }}
      onMouseEnter={onKeep}
      onMouseLeave={onLeave}
    >
      <div className={styles.popoverHead}>
        <strong className={styles.popoverTerm}>{term.content}</strong>
        <span className={styles.levelBadge} data-level={term.learningLevel}>
          {LEVEL_LABELS[term.learningLevel]}
        </span>
      </div>
      {term.count ? (
        <div className={styles.occurrences}>
          <span style={{ background: importanceColor(term.count) }} />
          {`${term.count} in this text`}
        </div>
      ) : null}
      <p className={styles.meaning}>
        {meaning || <em>No meaning yet</em>}
        {!term.meaning && suggestion && <span className={styles.hint}> (dictionary)</span>}
      </p>
      <div className={styles.popoverActions}>
        <button type="button" className="small" onClick={() => onChangeLevel(nextLevel(term.learningLevel))}>
          <ArrowUpIcon />
          Better
        </button>
        <button type="button" className="secondary small" onClick={() => onChangeLevel(previousLevel(term.learningLevel))}>
          <ArrowDownIcon />
          Worse
        </button>
      </div>
    </div>
  );
}

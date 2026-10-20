'use client';

import { importanceColor } from '@/lib/importance';
import { nextLevel, previousLevel, LEVEL_LABELS, type LearningLevel, type TextTermView } from '@/lib/types';
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
        <strong>{term.content}</strong>
        <span>{LEVEL_LABELS[term.learningLevel]}</span>
      </div>
      {term.count ? <div style={{ color: importanceColor(term.count) }}>{`${term.count} in this text`}</div> : null}
      <p className={styles.meaning}>
        {meaning || <em>No meaning yet</em>}
        {!term.meaning && suggestion && <span className={styles.hint}> (dictionary)</span>}
      </p>
      <div className="row">
        <button type="button" onClick={() => onChangeLevel(nextLevel(term.learningLevel))}>
          Better
        </button>
        <button type="button" onClick={() => onChangeLevel(previousLevel(term.learningLevel))}>
          Worse
        </button>
      </div>
    </div>
  );
}

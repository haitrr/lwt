'use client';

import { useState } from 'react';
import { importanceColor } from '@/lib/importance';
import { LearningLevel, LEVEL_LABELS, nextLevel, type TextTermView } from '@/lib/types';
import { ArrowUpIcon, CloseIcon, CopyIcon } from '@/components/icons';
import styles from './reader.module.css';

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
      className={styles.panel}
      onSubmit={(e) => {
        e.preventDefault();
        void save(level);
      }}
    >
      <div className={styles.popoverHead}>
        <button
          type="button"
          className={styles.termTitle}
          title="Copy"
          style={{ color: term.count ? importanceColor(term.count) : undefined }}
          onClick={() => void navigator.clipboard?.writeText(term.content)}
        >
          {`${term.content} (${term.count ?? '-'})`}
          <CopyIcon />
        </button>
        <button type="button" className="ghost small" onClick={onClose} aria-label="Close">
          <CloseIcon />
        </button>
      </div>
      <textarea
        rows={3}
        placeholder="Meaning"
        value={shownMeaning}
        onChange={(e) => setMeaning(e.target.value)}
        disabled={saving}
      />
      <div className={styles.panelActions}>
        <select aria-label="Level" value={level} onChange={(e) => setLevel(e.target.value as LearningLevel)} disabled={saving}>
          {EDITABLE_LEVELS.map((l) => (
            <option key={l} value={l}>
              {LEVEL_LABELS[l]}
            </option>
          ))}
        </select>
        <button type="button" className="secondary" disabled={saving} onClick={() => void save(nextLevel(level))}>
          <ArrowUpIcon />
          Better
        </button>
        <button type="submit" disabled={saving}>
          Save
        </button>
      </div>
    </form>
  );
}

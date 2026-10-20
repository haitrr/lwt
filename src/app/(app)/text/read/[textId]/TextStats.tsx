import { LearningLevel, LEVEL_LABELS, type TermCounts } from '@/lib/types';
import styles from './reader.module.css';

const SHOWN_LEVELS = [
  LearningLevel.Unknown,
  LearningLevel.Learning1,
  LearningLevel.Learning2,
  LearningLevel.Learning3,
  LearningLevel.Learning4,
  LearningLevel.Learning5,
];

type Segment = { key: string; label: string; value: number };

function SegmentBar({ segments, label }: { segments: Segment[]; label: string }) {
  const total = segments.reduce((sum, s) => sum + s.value, 0);
  if (!total) return null;
  return (
    <div className={styles.segments} role="img" aria-label={`${label}: ${segments.map((s) => `${s.label} ${s.value}`).join(', ')}`}>
      {segments
        .filter((s) => s.value > 0)
        .map((s) => (
          <div
            key={s.key}
            data-level={s.key}
            style={{ flexGrow: s.value }}
            title={`${s.label}: ${s.value} (${Math.round((s.value * 100) / total)}%)`}
          >
            {(s.value * 100) / total >= 8 ? `${s.label} ${Math.round((s.value * 100) / total)}%` : ''}
          </div>
        ))}
    </div>
  );
}

/** Share of known vs still-learning words, then the learning ones by level. */
export default function TextStats({ counts, termCount }: { counts: Required<TermCounts>; termCount: number }) {
  const known = counts[LearningLevel.WellKnown];
  const learning = termCount - counts[LearningLevel.Skipped] - counts[LearningLevel.Ignored] - known;
  return (
    <div className={styles.stats}>
      <SegmentBar
        label="Words"
        segments={[
          { key: 'learned', label: 'Learned', value: known },
          { key: 'learning', label: 'Learning', value: Math.max(learning, 0) },
        ]}
      />
      <SegmentBar label="Learning words by level" segments={SHOWN_LEVELS.map((l) => ({ key: l, label: LEVEL_LABELS[l], value: counts[l] }))} />
    </div>
  );
}

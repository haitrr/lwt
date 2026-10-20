import { LearningLevel, LEVEL_LABELS, type TermCounts, type TextItem } from '@/lib/types';
import { cn } from '@/lib/utils';

const LEVEL_COLUMNS: [string, LearningLevel][] = [
  ['UK', LearningLevel.Unknown],
  ['L1', LearningLevel.Learning1],
  ['L2', LearningLevel.Learning2],
  ['L3', LearningLevel.Learning3],
  ['L4', LearningLevel.Learning4],
  ['L5', LearningLevel.Learning5],
  ['WK', LearningLevel.WellKnown],
];

export function percentRead(text: TextItem) {
  if (!text.termCount || !text.bookmark) return 0;
  return Math.floor((text.bookmark / text.termCount) * 10000) / 100;
}

/** How the text's words split across learning levels: a stacked bar, then the count and share of each level. */
export function Vocabulary({ counts }: { counts: TermCounts }) {
  const total = Object.entries(counts)
    .filter(([l]) => l !== LearningLevel.Skipped && l !== LearningLevel.Ignored)
    .reduce((sum, [, n]) => sum + (n ?? 0), 0);
  const levels = LEVEL_COLUMNS.map(([label, level]) => ({ label, level, count: counts[level] ?? 0 }));
  const share = (n: number) => (total ? Math.round((n / total) * 100) : 0);
  return (
    <div className="flex min-w-0 flex-col gap-2">
      <div className="flex h-2 gap-0.5 overflow-hidden rounded-full bg-muted" aria-hidden>
        {levels
          .filter((l) => l.count)
          .map((l) => (
            <span key={l.level} data-level={l.level} className="bg-level" style={{ flexGrow: l.count }} />
          ))}
      </div>
      <ul className="flex flex-wrap gap-x-3 gap-y-1 text-xs tabular-nums">
        {levels.map((l) => (
          <li
            key={l.level}
            data-level={l.level}
            className={cn('flex items-center gap-1', !l.count && 'opacity-40')}
            title={LEVEL_LABELS[l.level]}
          >
            <span className="size-2 rounded-[2px] bg-level" />
            <span className="font-medium text-muted-foreground">{l.label}</span>
            <span>{l.count}</span>
            {l.count > 0 && <span className="text-muted-foreground">{`${share(l.count)}%`}</span>}
          </li>
        ))}
        <li className="flex items-center gap-1 text-muted-foreground" title="Ignored">
          <span className="font-medium">Ignored</span>
          <span>{counts[LearningLevel.Ignored] ?? 0}</span>
        </li>
      </ul>
    </div>
  );
}

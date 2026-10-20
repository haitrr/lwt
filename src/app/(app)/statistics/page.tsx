import { languageName } from '@/lib/languages';
import { LearningLevel, LEVEL_LABELS } from '@/lib/types';
import { requireSession } from '@/server/auth/session';
import { countTermsByLanguage, countTermsByLearningLevel } from '@/server/terms';
import { countTextsByLanguage } from '@/server/texts';
import styles from './statistics.module.css';

export const metadata = { title: 'Statistics' };

const LEARNED_LEVELS = [
  LearningLevel.Learning1,
  LearningLevel.Learning2,
  LearningLevel.Learning3,
  LearningLevel.Learning4,
  LearningLevel.Learning5,
  LearningLevel.WellKnown,
];

type Row = { key: string; label: string; value: number; level?: string };

function BarList({ title, rows }: { title: string; rows: Row[] }) {
  const max = Math.max(1, ...rows.map((r) => r.value));
  return (
    <figure className={`card ${styles.chart}`}>
      <figcaption>{title}</figcaption>
      {rows.length === 0 ? (
        <p className={styles.empty}>No data yet.</p>
      ) : (
        <table>
          <tbody>
            {rows.map((r) => (
              <tr key={r.key} title={`${r.label}: ${r.value}`}>
                <th scope="row">{r.label}</th>
                <td className={styles.barCell}>
                  <span
                    className={styles.bar}
                    data-level={r.level}
                    style={{ width: `${(r.value / max) * 100}%` }}
                  />
                </td>
                <td className={styles.value}>{r.value}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </figure>
  );
}

function byLanguage(counts: Record<string, number>): Row[] {
  return Object.entries(counts)
    .sort(([, a], [, b]) => b - a)
    .map(([code, value]) => ({ key: code, label: languageName(code), value }));
}

export default async function StatisticsPage() {
  const { userId } = await requireSession();
  const [textCounts, termCounts, levelCounts] = await Promise.all([
    countTextsByLanguage(userId),
    countTermsByLanguage(userId),
    countTermsByLearningLevel(userId),
  ]);
  const levels = LEARNED_LEVELS.filter((level) => levelCounts[level]).map((level) => ({
    key: level,
    label: LEVEL_LABELS[level],
    value: levelCounts[level],
    level,
  }));
  const sum = (counts: Record<string, number>) => Object.values(counts).reduce((a, b) => a + b, 0);
  const tiles = [
    { label: 'Texts', value: sum(textCounts) },
    { label: 'Learned terms', value: sum(termCounts) },
    { label: 'Well known', value: levelCounts[LearningLevel.WellKnown] ?? 0 },
    { label: 'Languages', value: Object.keys(textCounts).length },
  ];
  return (
    <>
      <div className="pageHeader">
        <h1>Statistics</h1>
      </div>
      <div className={styles.tiles}>
        {tiles.map((t) => (
          <div key={t.label} className={`card ${styles.tile}`}>
            <span>{t.label}</span>
            <strong>{t.value.toLocaleString('en')}</strong>
          </div>
        ))}
      </div>
      <div className={styles.grid}>
        <BarList title="Texts by language" rows={byLanguage(textCounts)} />
        <BarList title="Learned terms by language" rows={byLanguage(termCounts)} />
        <BarList title="Learned terms by level" rows={levels} />
      </div>
    </>
  );
}

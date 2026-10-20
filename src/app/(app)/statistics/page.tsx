import { languageName } from '@/lib/languages';
import { LearningLevel, LEVEL_LABELS } from '@/lib/types';
import { requireSession } from '@/server/auth/session';
import { countTermsByLanguage, countTermsByLearningLevel } from '@/server/terms';
import { countTextsByLanguage } from '@/server/texts';
import { cn } from '@/lib/utils';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

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
    <Card className="gap-4">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent>
        {rows.length === 0 ? (
          <p className="text-sm text-muted-foreground">No data yet.</p>
        ) : (
          <table className="w-full text-sm">
            <tbody>
              {rows.map((r) => (
                <tr key={r.key} title={`${r.label}: ${r.value}`}>
                  <th scope="row" className="py-1.5 pr-3 text-left font-normal whitespace-nowrap text-muted-foreground">
                    {r.label}
                  </th>
                  <td className="w-full">
                    <span
                      data-level={r.level}
                      className={cn('block h-2.5 min-w-0.5 rounded-full', r.level ? 'bg-level' : 'bg-primary')}
                      style={{ width: `${(r.value / max) * 100}%` }}
                    />
                  </td>
                  <td className="pl-3 text-right font-medium tabular-nums">{r.value}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </CardContent>
    </Card>
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
    <div className="flex flex-col gap-4">
      <h1 className="text-2xl font-semibold tracking-tight">Statistics</h1>
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {tiles.map((t) => (
          <Card key={t.label} className="gap-1 py-5">
            <CardHeader>
              <CardDescription>{t.label}</CardDescription>
              <CardTitle className="text-3xl tabular-nums">{t.value.toLocaleString('en')}</CardTitle>
            </CardHeader>
          </Card>
        ))}
      </div>
      <div className="grid items-start gap-4 md:grid-cols-2 xl:grid-cols-3">
        <BarList title="Texts by language" rows={byLanguage(textCounts)} />
        <BarList title="Learned terms by language" rows={byLanguage(termCounts)} />
        <BarList title="Learned terms by level" rows={levels} />
      </div>
    </div>
  );
}

import Link from 'next/link';
import { requireSession } from '@/server/auth/session';
import { LANGUAGES, languageName } from '@/lib/languages';
import { getTermCounts, listTexts } from '@/server/texts';
import { LearningLevel, type TermCounts, type TextItem } from '@/lib/types';
import TextFilter from './TextFilter';
import DeleteTextButton from './DeleteTextButton';
import RefreshWhileProcessing from './RefreshWhileProcessing';
import styles from './texts.module.css';

export const metadata = { title: 'Texts' };

const ITEMS_PER_PAGE = 10;

const LEVEL_COLUMNS: [string, LearningLevel][] = [
  ['UK', LearningLevel.Unknown],
  ['L1', LearningLevel.Learning1],
  ['L2', LearningLevel.Learning2],
  ['L3', LearningLevel.Learning3],
  ['L4', LearningLevel.Learning4],
  ['L5', LearningLevel.Learning5],
  ['WK', LearningLevel.WellKnown],
];

const relativeTime = new Intl.RelativeTimeFormat('en', { numeric: 'auto' });

function timeAgo(iso: string) {
  const date = new Date(iso);
  const days = Math.round((date.getTime() - Date.now()) / 86_400_000);
  if (Math.abs(days) >= 30) return relativeTime.format(Math.round(days / 30), 'month');
  return relativeTime.format(days, 'day');
}

function percentRead(text: TextItem) {
  if (!text.termCount || !text.bookmark) return 0;
  return Math.floor((text.bookmark / text.termCount) * 10000) / 100;
}

function Status({ text }: { text: TextItem }) {
  if (text.termCount === 0) return <span className={styles.processing}>Processing</span>;
  if (text.processedTermCount >= text.termCount) return <span className={styles.done}>Done</span>;
  const pct = Math.floor((text.processedTermCount * 100) / text.termCount);
  return <span className={styles.partial}>{`${text.processedTermCount}/${text.termCount} (${pct}%)`}</span>;
}

function LevelCount({ counts, level }: { counts: TermCounts; level: LearningLevel }) {
  const current = counts[level] ?? 0;
  const total = Object.entries(counts)
    .filter(([l]) => l !== LearningLevel.Skipped && l !== LearningLevel.Ignored)
    .reduce((sum, [, n]) => sum + (n ?? 0), 0);
  if (!current || !total) return <span>0</span>;
  return (
    <span className={styles.level} data-level={level}>
      {current}
      <br />
      {`${Math.round((current / total) * 100)}%`}
    </span>
  );
}

export default async function TextsPage({ searchParams }: PageProps<'/text'>) {
  const params = await searchParams;
  const page = Math.max(1, Number(params.page) || 1);
  const title = typeof params.title === 'string' ? params.title : '';
  const languageCode = typeof params.languageCode === 'string' ? params.languageCode : '';

  const session = await requireSession();
  const texts = await listTexts(session.userId, { title, languageCode }, page, ITEMS_PER_PAGE);
  const counts = await Promise.all(texts.items.map((t) => getTermCounts(t.id, session.userId)));
  const pageCount = Math.max(1, Math.ceil(texts.total / ITEMS_PER_PAGE));
  const pageHref = (p: number) => {
    const q = new URLSearchParams({ page: String(p) });
    if (title) q.set('title', title);
    if (languageCode) q.set('languageCode', languageCode);
    return `/text?${q}`;
  };

  return (
    <>
      <RefreshWhileProcessing
        active={texts.items.some((t) => t.termCount === 0 || t.processedTermCount < t.termCount)}
      />
      <div className={styles.toolbar}>
        <Link href="/text/new" className="button">
          Add text
        </Link>
        <TextFilter languages={LANGUAGES} title={title} languageCode={languageCode} />
      </div>
      <div className={styles.tableWrap}>
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Title</th>
              <th>Language</th>
              <th>Created</th>
              <th>Read</th>
              {LEVEL_COLUMNS.map(([label]) => (
                <th key={label}>{label}</th>
              ))}
              <th>Ignored</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {texts.items.map((text, i) => (
              <tr key={text.id}>
                <td className={styles.title}>{text.title}</td>
                <td>{languageName(text.languageCode)}</td>
                <td>{timeAgo(text.createdAt)}</td>
                <td>
                  <Link href={`/text/read/${text.id}`}>{`${percentRead(text)}%`}</Link>
                </td>
                {LEVEL_COLUMNS.map(([label, level]) => (
                  <td key={label}>
                    <LevelCount counts={counts[i]} level={level} />
                  </td>
                ))}
                <td>{counts[i][LearningLevel.Ignored] ?? 0}</td>
                <td>
                  <Status text={text} />
                </td>
                <td className={styles.actions}>
                  <Link href={`/text/${text.id}/edit`}>Edit</Link>
                  <DeleteTextButton id={text.id} title={text.title} />
                </td>
              </tr>
            ))}
            {texts.items.length === 0 && (
              <tr>
                <td colSpan={LEVEL_COLUMNS.length + 7}>No texts yet.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
      <nav className={styles.pagination}>
        {page > 1 && <Link href={pageHref(page - 1)}>Previous</Link>}
        <span>{`Page ${page} of ${pageCount}`}</span>
        {page < pageCount && <Link href={pageHref(page + 1)}>Next</Link>}
      </nav>
    </>
  );
}

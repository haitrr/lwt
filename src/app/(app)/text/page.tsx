import Link from 'next/link';
import { requireSession } from '@/server/auth/session';
import { LANGUAGES, languageName } from '@/lib/languages';
import { getTermCounts, listTexts } from '@/server/texts';
import { LearningLevel, LEVEL_LABELS, type TermCounts, type TextItem } from '@/lib/types';
import { BookIcon, ChevronLeftIcon, ChevronRightIcon, PencilIcon, PlusIcon } from '@/components/icons';
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
  if (text.termCount === 0) return <span className={`badge ${styles.processing}`}>Processing</span>;
  if (text.processedTermCount >= text.termCount) return <span className={`badge ${styles.done}`}>Done</span>;
  const pct = Math.floor((text.processedTermCount * 100) / text.termCount);
  return <span className={`badge ${styles.partial}`}>{`${text.processedTermCount}/${text.termCount} (${pct}%)`}</span>;
}

/** How the text's words split across learning levels: a stacked bar, then the count and share of each level. */
function Vocabulary({ counts }: { counts: TermCounts }) {
  const total = Object.entries(counts)
    .filter(([l]) => l !== LearningLevel.Skipped && l !== LearningLevel.Ignored)
    .reduce((sum, [, n]) => sum + (n ?? 0), 0);
  const levels = LEVEL_COLUMNS.map(([label, level]) => ({ label, level, count: counts[level] ?? 0 }));
  const share = (n: number) => (total ? Math.round((n / total) * 100) : 0);
  return (
    <div className={styles.vocabulary}>
      <div className={styles.levelBar} aria-hidden>
        {levels
          .filter((l) => l.count)
          .map((l) => (
            <span key={l.level} data-level={l.level} style={{ flexGrow: l.count }} />
          ))}
      </div>
      <ul className={styles.levels}>
        {levels.map((l) => (
          <li key={l.level} data-level={l.level} className={l.count ? undefined : styles.zero} title={LEVEL_LABELS[l.level]}>
            <span className={styles.levelLabel}>{l.label}</span>
            <span>{l.count}</span>
            {l.count > 0 && <span className={styles.levelShare}>{`${share(l.count)}%`}</span>}
          </li>
        ))}
        <li className={styles.ignored} title="Ignored">
          <span className={styles.levelLabel}>Ignored</span>
          <span>{counts[LearningLevel.Ignored] ?? 0}</span>
        </li>
      </ul>
    </div>
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
      <div className="pageHeader">
        <h1>Texts</h1>
        <Link href="/text/new" className="button">
          <PlusIcon />
          Add text
        </Link>
      </div>
      <TextFilter languages={LANGUAGES} title={title} languageCode={languageCode} />
      {texts.items.length === 0 ? (
        <div className={`card empty`}>
          <p>{title || languageCode ? 'No texts match this filter.' : 'No texts yet.'}</p>
        </div>
      ) : (
        <ul className={styles.list}>
          {texts.items.map((text, i) => (
            <li key={text.id} className={`card ${styles.item}`}>
              <div className={styles.main}>
                <div className={styles.heading}>
                  <Link href={`/text/read/${text.id}`} className={styles.title}>
                    {text.title}
                  </Link>
                  <Status text={text} />
                </div>
                <div className={styles.meta}>
                  <span className="badge">{languageName(text.languageCode)}</span>
                  <span>{`Created ${timeAgo(text.createdAt)}`}</span>
                </div>
                <div className={styles.read}>
                  <div className="progressTrack">
                    <div style={{ width: `${percentRead(text)}%` }} />
                  </div>
                  <Link href={`/text/read/${text.id}`}>{`${percentRead(text)}% read`}</Link>
                </div>
              </div>
              <Vocabulary counts={counts[i]} />
              <div className={styles.actions}>
                <Link href={`/text/read/${text.id}`} className="button small">
                  <BookIcon />
                  Read
                </Link>
                <Link href={`/text/${text.id}/edit`} className="button ghost small" aria-label={`Edit ${text.title}`} title="Edit">
                  <PencilIcon />
                </Link>
                <DeleteTextButton id={text.id} title={text.title} />
              </div>
            </li>
          ))}
        </ul>
      )}
      {pageCount > 1 && (
        <nav className={styles.pagination} aria-label="Pages">
          {page > 1 ? (
            <Link href={pageHref(page - 1)} className="button secondary small">
              <ChevronLeftIcon />
              Previous
            </Link>
          ) : (
            <span />
          )}
          <span className="subtle">{`Page ${page} of ${pageCount}`}</span>
          {page < pageCount ? (
            <Link href={pageHref(page + 1)} className="button secondary small">
              Next
              <ChevronRightIcon />
            </Link>
          ) : (
            <span />
          )}
        </nav>
      )}
    </>
  );
}

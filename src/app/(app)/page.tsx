import Link from 'next/link';
import { languageName } from '@/lib/languages';
import { requireSession } from '@/server/auth/session';
import { getLastReadText } from '@/server/texts';
import { BookIcon, PlusIcon } from '@/components/icons';
import styles from './home.module.css';

export const metadata = { title: 'Home' };

export default async function HomePage() {
  const session = await requireSession();
  const text = await getLastReadText(session);
  if (!text) {
    return (
      <div className={`card ${styles.welcome}`}>
        <h1>{`Welcome, ${session.userName}`}</h1>
        <p className="subtle">Nothing read yet. Add a text or pick one to start reading.</p>
        <div className={styles.actions}>
          <Link href="/text/new" className="button">
            <PlusIcon />
            Add text
          </Link>
          <Link href="/text" className="button secondary">
            <BookIcon />
            Browse texts
          </Link>
        </div>
      </div>
    );
  }
  const read = text.bookmark ?? 0;
  const percentage = text.termCount ? ((read * 100) / text.termCount).toFixed(2) : '0.00';
  return (
    <>
      <h1>{`Welcome back, ${session.userName}`}</h1>
      <section className={`card ${styles.continue}`}>
        <span className={styles.eyebrow}>Previously read</span>
        <h2 className={styles.title}>{text.title}</h2>
        <span className="badge">{languageName(text.languageCode)}</span>
        <div className={styles.progress}>
          <div className="progressTrack">
            <div style={{ width: `${percentage}%` }} />
          </div>
          <span className="subtle">{`Done: ${read}/${text.termCount} (${percentage}%)`}</span>
        </div>
        <div className={styles.actions}>
          <Link href={`/text/read/${text.id}`} className="button">
            <BookIcon />
            Continue reading
          </Link>
          <Link href="/text" className="button secondary">
            All texts
          </Link>
        </div>
      </section>
    </>
  );
}

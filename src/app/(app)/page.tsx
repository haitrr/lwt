import Link from 'next/link';
import { languageName } from '@/lib/languages';
import { requireSession } from '@/server/auth/session';
import { getLastReadText } from '@/server/texts';

export const metadata = { title: 'Home' };

export default async function HomePage() {
  const session = await requireSession();
  const text = await getLastReadText(session);
  if (!text) {
    return (
      <p>
        Nothing read yet. <Link href="/text">Pick a text</Link> to start.
      </p>
    );
  }
  const read = text.bookmark ?? 0;
  const percentage = text.termCount ? ((read * 100) / text.termCount).toFixed(2) : '0.00';
  return (
    <section>
      <h2>Previously read</h2>
      <div className="card">
        <h3>{text.title}</h3>
        <p>{languageName(text.languageCode)}</p>
        <p>{`Done: ${read}/${text.termCount} (${percentage}%)`}</p>
        <Link href={`/text/read/${text.id}`} className="button">
          Continue reading
        </Link>
      </div>
    </section>
  );
}

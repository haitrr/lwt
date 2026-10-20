import Link from 'next/link';

export default function Brand() {
  return (
    <Link href="/" className="brand" aria-label="Lwt home">
      <span className="brandMark" aria-hidden>
        L
      </span>
      <span className="brandName">Lwt</span>
    </Link>
  );
}

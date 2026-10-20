import Link from 'next/link';
import { logout } from '@/lib/auth';

export default function AppLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <>
      <header className="header">
        <nav>
          <Link href="/">Home</Link>
          <Link href="/text">Texts</Link>
          <Link href="/statistics">Statistics</Link>
          <Link href="/profile">Profile</Link>
        </nav>
        <form action={logout}>
          <button type="submit">Logout</button>
        </form>
      </header>
      <main>{children}</main>
    </>
  );
}

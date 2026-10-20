import { Suspense } from 'react';
import { logout } from '@/lib/auth';
import Brand from '@/components/Brand';
import NavLinks, { NavList } from '@/components/NavLinks';
import ThemeToggle from '@/components/ThemeToggle';
import { LogoutIcon } from '@/components/icons';

export default function AppLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <>
      <header className="header">
        <Brand />
        <Suspense fallback={<NavList />}>
          <NavLinks />
        </Suspense>
        <div className="headerActions">
          <ThemeToggle />
          <form action={logout}>
            <button type="submit" className="iconButton" aria-label="Logout" title="Logout">
              <LogoutIcon />
              <span className="navLabel">Logout</span>
            </button>
          </form>
        </div>
      </header>
      <main>{children}</main>
    </>
  );
}

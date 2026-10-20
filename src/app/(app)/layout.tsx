import { Suspense } from 'react';
import { LogOut } from 'lucide-react';
import { logout } from '@/lib/auth';
import Brand from '@/components/Brand';
import NavLinks, { NavList } from '@/components/NavLinks';
import ThemeToggle from '@/components/ThemeToggle';
import { Button } from '@/components/ui/button';

export default function AppLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <>
      <header className="sticky top-0 z-30 border-b bg-background/80 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="flex h-14 items-center gap-2 px-2 sm:gap-4 sm:px-4">
          <Brand hideName />
          <Suspense fallback={<NavList />}>
            <NavLinks />
          </Suspense>
          <div className="flex items-center gap-1">
            <ThemeToggle />
            <form action={logout}>
              <Button type="submit" variant="ghost" className="text-muted-foreground" aria-label="Logout" title="Logout">
                <LogOut />
                <span className="hidden sm:inline">Logout</span>
              </Button>
            </form>
          </div>
        </div>
      </header>
      <main className="mx-auto w-full max-w-6xl px-4 py-6 sm:py-8">{children}</main>
    </>
  );
}

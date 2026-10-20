'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { BookOpen, ChartColumn, House, User } from 'lucide-react';
import { Button } from '@/components/ui/button';

const LINKS = [
  { href: '/', label: 'Home', icon: House },
  { href: '/text', label: 'Texts', icon: BookOpen },
  { href: '/statistics', label: 'Statistics', icon: ChartColumn },
  { href: '/profile', label: 'Profile', icon: User },
];

// The pathname is request data, so the layout renders <NavList /> without it as the Suspense fallback.
export default function NavLinks() {
  return <NavList pathname={usePathname()} />;
}

export function NavList({ pathname }: { pathname?: string }) {
  return (
    <nav className="flex flex-1 justify-center gap-1 sm:justify-start">
      {LINKS.map(({ href, label, icon: Icon }) => {
        const active = pathname != null && (href === '/' ? pathname === '/' : pathname.startsWith(href));
        return (
          <Button
            key={href}
            asChild
            variant="ghost"
            className={active ? 'bg-accent text-accent-foreground' : 'text-muted-foreground'}
          >
            <Link href={href} aria-current={active ? 'page' : undefined} aria-label={label} title={label}>
              <Icon />
              <span className="hidden sm:inline">{label}</span>
            </Link>
          </Button>
        );
      })}
    </nav>
  );
}

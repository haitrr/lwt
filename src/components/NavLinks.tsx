'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { BookIcon, ChartIcon, HomeIcon, UserIcon } from './icons';

const LINKS = [
  { href: '/', label: 'Home', icon: <HomeIcon /> },
  { href: '/text', label: 'Texts', icon: <BookIcon /> },
  { href: '/statistics', label: 'Statistics', icon: <ChartIcon /> },
  { href: '/profile', label: 'Profile', icon: <UserIcon /> },
];

// The pathname is request data, so the layout renders <NavList /> without it as the Suspense fallback.
export default function NavLinks() {
  return <NavList pathname={usePathname()} />;
}

export function NavList({ pathname }: { pathname?: string }) {
  return (
    <nav className="nav">
      {LINKS.map(({ href, label, icon }) => {
        const active = pathname != null && (href === '/' ? pathname === '/' : pathname.startsWith(href));
        return (
          <Link key={href} href={href} aria-current={active ? 'page' : undefined} aria-label={label} title={label}>
            {icon}
            <span className="navLabel">{label}</span>
          </Link>
        );
      })}
    </nav>
  );
}

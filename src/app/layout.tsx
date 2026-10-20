import type { Metadata, Viewport } from 'next';
import { themeScript } from '@/components/ThemeToggle';
import './globals.css';

export const metadata: Metadata = {
  title: { default: 'Lwt', template: '%s · Lwt' },
  description: 'Learning with texts',
};

export const viewport: Viewport = {
  themeColor: [
    { media: '(prefers-color-scheme: light)', color: '#ffffff' },
    { media: '(prefers-color-scheme: dark)', color: '#171a21' },
  ],
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    // The theme script sets data-theme before hydration.
    <html lang="en" suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeScript }} />
      </head>
      <body>{children}</body>
    </html>
  );
}

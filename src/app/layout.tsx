import type { Metadata, Viewport } from 'next';
import { GeistSans } from 'geist/font/sans';
import { themeScript } from '@/components/theme-script';
import './globals.css';

export const metadata: Metadata = {
  title: { default: 'Lwt', template: '%s · Lwt' },
  description: 'Learning with texts',
};

export const viewport: Viewport = {
  themeColor: [
    { media: '(prefers-color-scheme: light)', color: '#ffffff' },
    { media: '(prefers-color-scheme: dark)', color: '#0a0a0a' },
  ],
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    // The theme script adds the dark class before hydration.
    <html lang="en" className={GeistSans.variable} suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeScript }} />
      </head>
      <body className="min-h-dvh font-sans">{children}</body>
    </html>
  );
}

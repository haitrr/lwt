'use client';

import { useSyncExternalStore } from 'react';
import { MoonIcon, SunIcon } from './icons';

const STORAGE_KEY = 'lwt-theme';

// Runs before the page paints, so a saved theme doesn't flash the system one first.
export const themeScript = `try{var t=localStorage.getItem('${STORAGE_KEY}');if(t)document.documentElement.dataset.theme=t}catch(e){}`;

const listeners = new Set<() => void>();
const subscribe = (listener: () => void) => {
  listeners.add(listener);
  const media = matchMedia('(prefers-color-scheme: dark)');
  media.addEventListener('change', listener);
  return () => {
    listeners.delete(listener);
    media.removeEventListener('change', listener);
  };
};
const isDark = () =>
  (document.documentElement.dataset.theme ?? (matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light')) ===
  'dark';

export default function ThemeToggle() {
  const dark = useSyncExternalStore(subscribe, isDark, () => false);
  const toggle = () => {
    const next = dark ? 'light' : 'dark';
    document.documentElement.dataset.theme = next;
    try {
      localStorage.setItem(STORAGE_KEY, next);
    } catch {}
    listeners.forEach((l) => l());
  };
  return (
    <button
      type="button"
      className="iconButton"
      onClick={toggle}
      aria-label={dark ? 'Use light theme' : 'Use dark theme'}
      title={dark ? 'Light theme' : 'Dark theme'}
    >
      {dark ? <SunIcon /> : <MoonIcon />}
    </button>
  );
}

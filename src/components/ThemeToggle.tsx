'use client';

import { Monitor, Moon, Sun } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useSyncExternalStore } from 'react';

type Theme = 'light' | 'dark' | 'system';

import { STORAGE_KEY } from './theme-script';

const listeners = new Set<() => void>();
const media = () => matchMedia('(prefers-color-scheme: dark)');

function readTheme(): Theme {
  try {
    const t = localStorage.getItem(STORAGE_KEY);
    return t === 'light' || t === 'dark' ? t : 'system';
  } catch {
    return 'system';
  }
}

function apply(theme: Theme) {
  const dark = theme === 'dark' || (theme === 'system' && media().matches);
  document.documentElement.classList.toggle('dark', dark);
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  const onSystemChange = () => {
    apply(readTheme());
    listener();
  };
  media().addEventListener('change', onSystemChange);
  return () => {
    listeners.delete(listener);
    media().removeEventListener('change', onSystemChange);
  };
}

export default function ThemeToggle() {
  const theme = useSyncExternalStore(subscribe, readTheme, () => 'system' as Theme);
  const choose = (value: string) => {
    const next = value as Theme;
    try {
      localStorage.setItem(STORAGE_KEY, next);
    } catch {}
    apply(next);
    listeners.forEach((l) => l());
  };
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" aria-label="Theme" title="Theme">
          <Sun className="scale-100 rotate-0 transition-all dark:scale-0 dark:-rotate-90" />
          <Moon className="absolute scale-0 rotate-90 transition-all dark:scale-100 dark:rotate-0" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuRadioGroup value={theme} onValueChange={choose}>
          <DropdownMenuRadioItem value="light">
            <Sun />
            Light
          </DropdownMenuRadioItem>
          <DropdownMenuRadioItem value="dark">
            <Moon />
            Dark
          </DropdownMenuRadioItem>
          <DropdownMenuRadioItem value="system">
            <Monitor />
            System
          </DropdownMenuRadioItem>
        </DropdownMenuRadioGroup>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

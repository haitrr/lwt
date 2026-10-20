'use client';

import { useTransition } from 'react';
import { deleteText } from '@/lib/texts';

export default function DeleteTextButton({ id, title }: { id: number; title: string }) {
  const [pending, startTransition] = useTransition();
  const onClick = () => {
    if (confirm(`Delete "${title}" forever?`)) {
      startTransition(() => deleteText(id));
    }
  };
  return (
    <button type="button" className="danger" onClick={onClick} disabled={pending}>
      Delete
    </button>
  );
}

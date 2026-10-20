'use client';

import { useTransition } from 'react';
import { deleteText } from '@/lib/texts';
import { TrashIcon } from '@/components/icons';

export default function DeleteTextButton({ id, title }: { id: number; title: string }) {
  const [pending, startTransition] = useTransition();
  const onClick = () => {
    if (confirm(`Delete "${title}" forever?`)) {
      startTransition(() => deleteText(id));
    }
  };
  return (
    <button
      type="button"
      className="danger small"
      onClick={onClick}
      disabled={pending}
      aria-label={`Delete ${title}`}
      title="Delete"
    >
      <TrashIcon />
    </button>
  );
}

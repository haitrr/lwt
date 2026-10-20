import { describe, expect, it, vi } from 'vitest';
import { LearningLevel } from '@/lib/types';

// Each query the function runs answers with the next of these, whatever its shape.
let results: unknown[][] = [];

vi.mock('server-only', () => ({}));
vi.mock('./db', async () => {
  const { schema } = await vi.importActual<typeof import('./db')>('./db');
  const query = (): unknown =>
    new Proxy(() => {}, {
      get: (_, key) => (key === 'then' ? (resolve: (rows: unknown[]) => void) => resolve(results.shift()!) : query),
      apply: () => query(),
    });
  return { schema, getDb: query };
});
const { getTextTerms } = await import('./texts');

describe('getTextTerms', () => {
  it('sends the stored meaning for every level, so saving the term panel keeps it', async () => {
    results = [
      [
        { content: 'hello', termId: 1, index: 0, learningLevel: LearningLevel.WellKnown, meaning: 'xin chào' },
        { content: 'world', termId: 2, index: 1, learningLevel: LearningLevel.Ignored, meaning: 'thế giới' },
        { content: 'cat', termId: 3, index: 2, learningLevel: LearningLevel.Learning1, meaning: 'mèo' },
        { content: ' ', termId: null, index: 3, learningLevel: null, meaning: null },
      ],
      [
        { termId: 1, n: 2 },
        { termId: 2, n: 1 },
        { termId: 3, n: 4 },
      ],
    ];
    const terms = await getTextTerms(10, 7, 0, 3);
    expect(terms.map((t) => t.meaning)).toEqual(['xin chào', 'thế giới', 'mèo', null]);
    expect(terms.map((t) => t.learningLevel)).toEqual([
      LearningLevel.WellKnown,
      LearningLevel.Ignored,
      LearningLevel.Learning1,
      LearningLevel.Skipped,
    ]);
    expect(terms.map((t) => t.count)).toEqual([2, 1, 4, null]);
  });
});

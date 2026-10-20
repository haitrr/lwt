export type Language = {
  name: string;
  code: string;
  speakCode: string;
  /** Whether texts can be written in it, i.e. the server has a splitter for it. */
  supportsTexts: boolean;
};

export type TextItem = {
  id: number;
  title: string;
  languageCode: string;
  termCount: number;
  processedTermCount: number;
  bookmark: number | null;
  /** Why splitting gave up; null unless it failed. */
  processingError: string | null;
  createdAt: string;
};

export type TextList = {
  total: number;
  items: TextItem[];
};

export type TextEditDetail = {
  title: string;
  languageCode: string;
  content: string;
};

export const LearningLevel = {
  Skipped: 'skipped',
  Ignored: 'ignored',
  Unknown: 'unknown',
  Learning1: 'learning-1',
  Learning2: 'learning-2',
  Learning3: 'learning-3',
  Learning4: 'learning-4',
  Learning5: 'learning-5',
  WellKnown: 'well-known',
} as const;

export type LearningLevel = (typeof LearningLevel)[keyof typeof LearningLevel];

export type TermCounts = Partial<Record<LearningLevel, number>>;

export type TextReadInfo = {
  id: number;
  title: string;
  languageCode: string;
  bookmark: number | null;
  termCount: number;
  processedTermCount: number;
};

/** One token of a text in reading order; id/meaning/count are set when it is a real term. */
export type TextTermView = {
  id: number | null;
  content: string;
  learningLevel: LearningLevel;
  index: number;
  meaning: string | null;
  count: number | null;
};

/** A dictionary lookup for a term: still running, its meaning, no entry, or the dictionary could not be asked. */
export type DictionaryLookup =
  | { status: 'loading' }
  | { status: 'found'; meaning: string }
  | { status: 'missing' }
  | { status: 'unavailable' };

export type TermView = {
  id: number;
  content: string;
  meaning: string;
  learningLevel: LearningLevel;
  languageCode: string;
};

export const LEARNING_LEVELS = Object.values(LearningLevel);

export function isLearningLevel(value: string): value is LearningLevel {
  return (LEARNING_LEVELS as string[]).includes(value);
}

const LEVEL_ORDER: LearningLevel[] = [
  LearningLevel.Ignored,
  LearningLevel.Unknown,
  LearningLevel.Learning1,
  LearningLevel.Learning2,
  LearningLevel.Learning3,
  LearningLevel.Learning4,
  LearningLevel.Learning5,
  LearningLevel.WellKnown,
];

/** Unknown through learning-5: terms still being learned. */
export function isLearningTerm(level: LearningLevel) {
  return LEVEL_ORDER.indexOf(level) >= 1 && LEVEL_ORDER.indexOf(level) <= 6;
}

export function nextLevel(level: LearningLevel): LearningLevel {
  const i = LEVEL_ORDER.indexOf(level);
  return i >= 1 && i < LEVEL_ORDER.length - 1 ? LEVEL_ORDER[i + 1] : level;
}

export function previousLevel(level: LearningLevel): LearningLevel {
  const i = LEVEL_ORDER.indexOf(level);
  return i >= 2 ? LEVEL_ORDER[i - 1] : i === 1 ? LearningLevel.Ignored : level;
}

export const LEVEL_LABELS: Record<LearningLevel, string> = {
  [LearningLevel.Skipped]: 'Skipped',
  [LearningLevel.Ignored]: 'Ignored',
  [LearningLevel.Unknown]: 'Unknown',
  [LearningLevel.Learning1]: 'Learning 1',
  [LearningLevel.Learning2]: 'Learning 2',
  [LearningLevel.Learning3]: 'Learning 3',
  [LearningLevel.Learning4]: 'Learning 4',
  [LearningLevel.Learning5]: 'Learning 5',
  [LearningLevel.WellKnown]: 'Well known',
};

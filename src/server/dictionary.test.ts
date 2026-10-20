import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

// Rows the built-in dictionary query answers with.
let builtIn: { word: string; meaning: string }[] = [];

vi.mock('server-only', () => ({}));
vi.mock('./languages/japanese', () => ({ japaneseBaseForm: async () => null }));
vi.mock('./db', async () => {
  const { schema } = await vi.importActual<typeof import('./db')>('./db');
  return { schema, getDb: () => ({ select: () => ({ from: () => ({ where: async () => builtIn }) }) }) };
});
const { lookUpMeaning } = await import('./dictionary');

const translator = { key: 'test-key', region: 'eastasia' };
const fetchMock = vi.fn<typeof fetch>();

const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status });

describe('lookUpMeaning', () => {
  beforeEach(() => {
    builtIn = [];
    vi.stubGlobal('fetch', fetchMock);
  });

  afterEach(() => {
    fetchMock.mockReset();
    vi.unstubAllGlobals();
  });

  it('uses the built-in dictionary when it has the word', async () => {
    builtIn = [{ word: 'speedometer', meaning: 'an instrument that shows speed' }];
    expect(await lookUpMeaning('speedometers', 'en', 'en', translator)).toBe('an instrument that shows speed');
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it.each([
    ['trotted', 'trot'],
    ['planned', 'plan'],
    ['bigger', 'big'],
    ['hottest', 'hot'],
  ])('finds %s under %s', async (word, base) => {
    builtIn = [{ word: base, meaning: 'found' }];
    expect(await lookUpMeaning(word, 'en', 'en', null)).toBe('found');
  });

  it('falls back to Microsoft, grouping translations by part of speech', async () => {
    fetchMock.mockResolvedValueOnce(
      json([
        { translations: [] },
        {
          translations: [
            { displayTarget: 'hỗ trợ', posTag: 'VERB' },
            { displayTarget: 'chỗ dựa', posTag: 'NOUN' },
            { displayTarget: 'bảo đảm', posTag: 'VERB' },
          ],
        },
      ]),
    );
    expect(await lookUpMeaning('backstopped', 'en', 'vi', translator)).toBe('verb: hỗ trợ, bảo đảm\nnoun: chỗ dựa');

    const [input, init] = fetchMock.mock.calls[0];
    expect(String(input)).toContain('microsofttranslator.com/dictionary/lookup');
    expect(String(input)).toContain('from=en&to=vi');
    const texts = (JSON.parse(String(init?.body)) as { Text: string }[]).map((t) => t.Text);
    expect(texts.slice(0, 2)).toEqual(['backstopped', 'backstop']);
    expect(init?.headers).toMatchObject({ 'Ocp-Apim-Subscription-Key': 'test-key', 'Ocp-Apim-Subscription-Region': 'eastasia' });
  });

  it("uses Microsoft's code for Chinese", async () => {
    fetchMock.mockResolvedValueOnce(json([{ translations: [{ displayTarget: 'speed', posTag: 'NOUN' }] }]));
    await lookUpMeaning('速度', 'zh', 'en', translator);
    expect(String(fetchMock.mock.calls[0][0])).toContain('from=zh-Hans&to=en');
  });

  it('is not found when no dictionary has the word', async () => {
    fetchMock.mockResolvedValueOnce(json([{ translations: [] }]));
    expect(await lookUpMeaning('zzxq', 'en', 'vi', translator)).toBeNull();
  });

  it('is not found when Microsoft has no dictionary for the language pair', async () => {
    fetchMock.mockResolvedValueOnce(json({ error: { code: 400036 } }, 400));
    expect(await lookUpMeaning('xin chào', 'vi', 'ja', translator)).toBeNull();
  });

  it('is unavailable when Microsoft could not be asked', async () => {
    fetchMock.mockRejectedValueOnce(new Error('timeout'));
    expect(await lookUpMeaning('backstop', 'en', 'vi', translator)).toBeUndefined();
  });

  it('skips Microsoft when the user has no key', async () => {
    expect(await lookUpMeaning('backstop', 'en', 'vi', null)).toBeNull();
    expect(fetchMock).not.toHaveBeenCalled();
  });
});

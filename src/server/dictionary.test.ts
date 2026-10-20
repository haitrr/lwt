import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('server-only', () => ({}));
const { lookUpMeaning } = await import('./dictionary');

const LWT_ROOT = 'https://lwt-dictionary.example/';
const translator = { key: 'test-key', region: 'eastasia' };
const fetchMock = vi.fn<typeof fetch>();

const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status });
const isMicrosoft = (input: Parameters<typeof fetch>[0]) => String(input).includes('microsofttranslator.com');

describe('lookUpMeaning', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock);
    vi.stubEnv('LWT_DICTIONARY_API_ROOT', LWT_ROOT);
  });

  afterEach(() => {
    fetchMock.mockReset();
    vi.unstubAllGlobals();
    vi.unstubAllEnvs();
  });

  it('uses the old dictionary when it has the word', async () => {
    fetchMock.mockResolvedValueOnce(json({ meaning: 'đồng hồ tốc độ' }));
    expect(await lookUpMeaning('speedometers', 'en', 'vi', translator)).toBe('đồng hồ tốc độ');
    expect(String(fetchMock.mock.calls[0][0])).toBe(`${LWT_ROOT}dictionary/speedometer?fromLang=en&toLang=vi`);
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it('falls back to Microsoft, grouping translations by part of speech', async () => {
    fetchMock.mockResolvedValueOnce(json({ Message: 'Term not found.' }, 404)).mockResolvedValueOnce(
      json([
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

    const [input, init] = fetchMock.mock.calls[1];
    expect(isMicrosoft(input)).toBe(true);
    expect(String(input)).toContain('from=en&to=vi');
    expect(init?.body).toBe(JSON.stringify([{ Text: 'backstop' }]));
    expect(init?.headers).toMatchObject({ 'Ocp-Apim-Subscription-Key': 'test-key', 'Ocp-Apim-Subscription-Region': 'eastasia' });
  });

  it("uses Microsoft's code for Chinese", async () => {
    vi.stubEnv('LWT_DICTIONARY_API_ROOT', '');
    fetchMock.mockResolvedValueOnce(json([{ translations: [{ displayTarget: 'speed', posTag: 'NOUN' }] }]));
    await lookUpMeaning('速度', 'zh', 'en', translator);
    expect(String(fetchMock.mock.calls[0][0])).toContain('from=zh-Hans&to=en');
  });

  it('is not found when no dictionary has the word', async () => {
    fetchMock.mockResolvedValueOnce(json({}, 404)).mockResolvedValueOnce(json([{ translations: [] }]));
    expect(await lookUpMeaning('zzxq', 'en', 'vi', translator)).toBeNull();
  });

  it('is not found when Microsoft has no dictionary for the language pair', async () => {
    fetchMock.mockResolvedValueOnce(json({}, 404)).mockResolvedValueOnce(json({ error: { code: 400036 } }, 400));
    expect(await lookUpMeaning('xin chào', 'vi', 'ja', translator)).toBeNull();
  });

  it('is unavailable when a dictionary that might have the word could not be asked', async () => {
    fetchMock.mockResolvedValueOnce(json({}, 404)).mockRejectedValueOnce(new Error('timeout'));
    expect(await lookUpMeaning('backstop', 'en', 'vi', translator)).toBeUndefined();
  });

  it('skips Microsoft when the user has no key', async () => {
    fetchMock.mockResolvedValueOnce(json({}, 404));
    expect(await lookUpMeaning('backstop', 'en', 'vi', null)).toBeNull();
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it('is unavailable when no dictionary is configured', async () => {
    vi.stubEnv('LWT_DICTIONARY_API_ROOT', '');
    expect(await lookUpMeaning('backstop', 'en', 'vi', null)).toBeUndefined();
    expect(fetchMock).not.toHaveBeenCalled();
  });
});

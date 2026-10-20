import path from 'node:path';
import kuromoji from 'kuromoji';

// The .NET API sent Japanese text to a MeCab (IPADIC) service; kuromoji uses the same IPADIC
// dictionary in-process. MeCab drops ASCII spaces, tabs and newlines (but keeps \r and full-width spaces), so this does too.

let tokenizer: Promise<kuromoji.Tokenizer<kuromoji.IpadicFeatures>> | undefined;

function getTokenizer() {
  tokenizer ??= new Promise((resolve, reject) => {
    kuromoji
      .builder({ dicPath: path.join(process.cwd(), 'node_modules/kuromoji/dict') })
      .build((err, t) => (err ? reject(err) : resolve(t)));
  });
  return tokenizer;
}

export async function cutJapanese(text: string): Promise<string[]> {
  const t = await getTokenizer();
  return t
    .tokenize(text)
    .map((token) => (/^\s+$/.test(token.surface_form) ? token.surface_form.replace(/[ \t\n]/g, '') : token.surface_form))
    .filter((surface) => surface !== '');
}

/** Dictionary form of a single word, e.g. 食べ -> 食べる, or null when it isn't one word kuromoji knows. */
export async function japaneseBaseForm(word: string): Promise<string | null> {
  const tokens = (await getTokenizer()).tokenize(word);
  if (tokens.length !== 1 || tokens[0].basic_form === '*') return null;
  return tokens[0].basic_form;
}

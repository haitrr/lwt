import { readFileSync } from 'node:fs';
import path from 'node:path';

// Port of the no-HMM path of Jieba.Net (JiebaSegmenter.Cut(text, cutAll: false, hmm: false)),
// using the same dictionary the .NET API shipped, so existing texts split identically.

const RE_HAN = /([一-鿕a-zA-Z0-9+#&._%-]+)/;
const RE_SKIP = /(\r\n|\s)/;
const RE_ENGLISH_CHAR = /^[a-zA-Z0-9]$/;

type Dictionary = { freq: Map<string, number>; words: Set<string>; logTotal: number };

let dictionary: Dictionary | undefined;

function loadDictionary(): Dictionary {
  if (dictionary) return dictionary;
  const file = path.join(process.cwd(), 'src/server/languages/data/jieba-dict.txt');
  const freq = new Map<string, number>();
  const words = new Set<string>();
  let total = 0;
  for (const line of readFileSync(file, 'utf8').split('\n')) {
    const [word, count] = line.trim().split(' ');
    if (!word) continue;
    const n = Number(count) || 0;
    freq.set(word, n);
    words.add(word);
    total += n;
    // Every prefix is kept with frequency 0 so DAG building knows when to keep extending.
    for (let i = 1; i < word.length; i++) {
      const prefix = word.slice(0, i);
      if (!freq.has(prefix)) freq.set(prefix, 0);
    }
  }
  dictionary = { freq, words, logTotal: Math.log(total) };
  return dictionary;
}

function buildDag(sentence: string, { freq, words }: Dictionary) {
  const dag: number[][] = [];
  for (let k = 0; k < sentence.length; k++) {
    const ends: number[] = [];
    let i = k;
    let frag = sentence[k];
    while (i < sentence.length && freq.has(frag)) {
      if (words.has(frag)) ends.push(i);
      i++;
      frag = sentence.slice(k, i + 1);
    }
    dag.push(ends.length ? ends : [k]);
  }
  return dag;
}

function cutDagWithoutHmm(sentence: string): string[] {
  const dict = loadDictionary();
  const { freq, logTotal } = dict;
  const dag = buildDag(sentence, dict);
  const n = sentence.length;
  const routeEnd = new Array<number>(n + 1);
  const routeProb = new Array<number>(n + 1);
  routeProb[n] = 0;
  for (let i = n - 1; i >= 0; i--) {
    let bestEnd = -1;
    let bestProb = -Infinity;
    for (const x of dag[i]) {
      const prob = Math.log(freq.get(sentence.slice(i, x + 1)) || 1) - logTotal + routeProb[x + 1];
      if (prob > bestProb) {
        bestProb = prob;
        bestEnd = x;
      }
    }
    routeEnd[i] = bestEnd;
    routeProb[i] = bestProb;
  }

  const words: string[] = [];
  let buf = '';
  for (let x = 0; x < n; ) {
    const y = routeEnd[x] + 1;
    const word = sentence.slice(x, y);
    if (y - x === 1 && RE_ENGLISH_CHAR.test(word)) {
      buf += word;
    } else {
      if (buf) {
        words.push(buf);
        buf = '';
      }
      words.push(word);
    }
    x = y;
  }
  if (buf) words.push(buf);
  return words;
}

export function cutChinese(text: string): string[] {
  const result: string[] = [];
  for (const block of text.split(RE_HAN)) {
    if (!block) continue;
    if (RE_HAN.test(block)) {
      result.push(...cutDagWithoutHmm(block));
      continue;
    }
    for (const part of block.split(RE_SKIP)) {
      if (!part) continue;
      if (RE_SKIP.test(part)) result.push(part);
      else result.push(...part);
    }
  }
  return result;
}

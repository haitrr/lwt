#!/usr/bin/env node
// Builds the dictionaries the app ships with into dictionary/<from>-<to>.tsv.gz plus dictionary/manifest.json.
// The app loads them into Postgres at startup (src/server/dictionary/import.ts), so lookups need no network.
//
//   zh -> en  CC-CEDICT (CC BY-SA 4.0), from the cedict-json package
//   ja -> en  JMdict (CC BY-SA 4.0, EDRDG), from a pinned jmdict-simplified release
//   en -> en  WordNet 3.1 (WordNet license), from the wordnet-db package
//
// Usage: npm run build-dictionary   (JMDICT_FILE=path/to/jmdict-eng.json(.tgz) skips the download)

import { createHash } from 'node:crypto';
import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { createRequire } from 'node:module';
import path from 'node:path';
import { gunzipSync, gzipSync } from 'node:zlib';

const require = createRequire(import.meta.url);
const OUT_DIR = path.join(import.meta.dirname, '..', 'dictionary');

// Bump together: a new release is published every week at github.com/scriptin/jmdict-simplified/releases.
const JMDICT_TAG = '3.6.2+20261019180022';
const JMDICT_SHA256 = '1dca86908663dcddb8c3bcff9dc5e2bc73037bbdeed9c705c896a3845e7635a5';
const JMDICT_URL = `https://github.com/scriptin/jmdict-simplified/releases/download/${encodeURIComponent(JMDICT_TAG)}/${encodeURIComponent(`jmdict-eng-${JMDICT_TAG}.json.tgz`)}`;

// Enough to recognize the word in the reader; a suggestion becomes the term's meaning when saved.
const MAX_LINES = 3;
const MAX_SENSES = 4;

// ---------- CC-CEDICT ----------

const TONE_MARKS = { a: 'āáǎà', e: 'ēéěè', i: 'īíǐì', o: 'ōóǒò', u: 'ūúǔù', ü: 'ǖǘǚǜ' };

/** "zhong1 guo2" -> "zhōng guó", "lu:4" -> "lǜ". */
export function toneMarks(pinyin) {
  return pinyin
    .split(' ')
    .map((syllable) => {
      const m = /^([a-zü:]+)([1-5])$/i.exec(syllable);
      if (!m) return syllable.replace(/u:/g, 'ü').replace(/U:/g, 'Ü');
      const letters = m[1].replace(/u:/g, 'ü').replace(/U:/g, 'Ü');
      const tone = Number(m[2]);
      if (tone === 5) return letters;
      const lower = letters.toLowerCase();
      // a or e takes the mark, then the o of "ou", otherwise the last vowel.
      let at = lower.search(/[ae]/);
      if (at < 0) at = lower.indexOf('ou');
      if (at < 0) at = Math.max(...[...'iouü'].map((v) => lower.lastIndexOf(v)));
      if (at < 0) return letters;
      const marked = TONE_MARKS[lower[at]][tone - 1];
      return letters.slice(0, at) + (letters[at] === lower[at] ? marked : marked.toUpperCase()) + letters.slice(at + 1);
    })
    .join(' ');
}

function buildChinese() {
  const entries = require('cedict-json');
  const byWord = new Map();
  const add = (word, line) => {
    const lines = byWord.get(word) ?? [];
    if (!lines.includes(line)) lines.push(line);
    byWord.set(word, lines);
  };
  // Common words before proper nouns (capitalized pinyin) and variant notes.
  const rank = (e) => (/^[A-Z]/.test(e.pinyin) ? 1 : 0) + (e.english.every((d) => /^(old )?variant of /.test(d)) ? 2 : 0);
  const sorted = entries.map((e, i) => ({ e, i, r: rank(e) })).sort((a, b) => a.r - b.r || a.i - b.i);
  for (const { e } of sorted) {
    const line = `${toneMarks(e.pinyin)}: ${e.english.slice(0, MAX_SENSES + 1).join('; ')}`;
    add(e.simplified, line);
    if (e.traditional !== e.simplified) add(e.traditional, line);
  }
  return byWord;
}

// ---------- JMdict ----------

function untarFirstFile(tar) {
  const size = parseInt(tar.subarray(124, 136).toString('latin1').replace(/\0.*$/, '').trim(), 8);
  return tar.subarray(512, 512 + size);
}

async function loadJmdict() {
  const file = process.env.JMDICT_FILE;
  let data;
  if (file) {
    data = readFileSync(file);
  } else {
    console.log(`Downloading ${JMDICT_URL}`);
    const res = await fetch(JMDICT_URL);
    if (!res.ok) throw new Error(`JMdict download failed: ${res.status}`);
    data = Buffer.from(await res.arrayBuffer());
    const sha = createHash('sha256').update(data).digest('hex');
    if (sha !== JMDICT_SHA256) throw new Error(`JMdict checksum mismatch: ${sha}`);
  }
  if (data[0] === 0x1f && data[1] === 0x8b) data = untarFirstFile(gunzipSync(data));
  return JSON.parse(data.toString('utf8'));
}

const applies = (list, text) => list.includes('*') || list.includes(text);

export function buildJapanese(jmdict) {
  // Per word: entries in priority order (common first), each with the line to show for it.
  const byWord = new Map();
  const words = jmdict.words
    .map((w, i) => ({ w, i, common: w.kanji.some((k) => k.common) || w.kana.some((k) => k.common) }))
    .sort((a, b) => Number(b.common) - Number(a.common) || a.i - b.i);
  for (const { w } of words) {
    const senses = (form, isKana) =>
      w.sense
        .filter((s) => (isKana ? applies(s.appliesToKana, form) : applies(s.appliesToKanji, form)))
        .map((s) => s.gloss.map((g) => g.text).join(', '))
        .filter(Boolean)
        .slice(0, MAX_SENSES);
    const add = (form, line) => {
      if (!line) return;
      const lines = byWord.get(form) ?? [];
      if (!lines.includes(line)) lines.push(line);
      byWord.set(form, lines);
    };
    for (const k of w.kanji) {
      const readings = w.kana.filter((r) => applies(r.appliesToKanji, k.text)).map((r) => r.text);
      const s = senses(k.text, false);
      if (s.length) add(k.text, `${readings.slice(0, 2).join('・')}: ${s.join('; ')}`);
    }
    for (const r of w.kana) {
      const s = senses(r.text, true);
      if (!s.length) continue;
      // A kana form usually has a kanji spelling worth showing; "usually written in kana" words don't need one.
      const kanji = w.kanji.filter((k) => !r.appliesToKanji.length || applies(r.appliesToKanji, k.text));
      const usuallyKana = w.sense.some((x) => x.misc.includes('uk'));
      add(r.text, kanji.length && !usuallyKana ? `${kanji[0].text}: ${s.join('; ')}` : s.join('; '));
    }
  }
  return byWord;
}

// ---------- WordNet ----------

const POS = [
  ['noun', 'n.'],
  ['verb', 'v.'],
  ['adj', 'adj.'],
  ['adv', 'adv.'],
];

function readGloss(data, offset) {
  // The gloss follows "| "; drop the quoted examples after the definitions.
  const line = data.toString('latin1', offset, data.indexOf(10, offset));
  const gloss = line.slice(line.indexOf('| ') + 2).trim();
  return gloss.split(/; "|^"/)[0].trim();
}

function buildEnglish() {
  const dir = path.join(path.dirname(require.resolve('wordnet-db/package.json')), 'dict');
  const senses = new Map(); // lemma -> [{ tagged, label, glosses }]
  for (const [file, label] of POS) {
    const data = readFileSync(path.join(dir, `data.${file}`));
    for (const line of readFileSync(path.join(dir, `index.${file}`), 'latin1').split('\n')) {
      if (!line || line.startsWith(' ')) continue;
      const parts = line.trim().split(' ');
      const lemma = parts[0].replace(/_/g, ' ');
      const pointerCount = Number(parts[3]);
      const tagged = Number(parts[5 + pointerCount]);
      const offsets = parts.slice(6 + pointerCount).map(Number);
      const glosses = offsets.slice(0, 2).map((o) => readGloss(data, o));
      const list = senses.get(lemma) ?? [];
      list.push({ tagged, label, glosses });
      senses.set(lemma, list);
    }
  }
  const byWord = new Map();
  for (const [lemma, list] of senses) {
    // The part of speech used most in the tagged corpus first.
    list.sort((a, b) => b.tagged - a.tagged);
    byWord.set(
      lemma,
      list.flatMap((s) => s.glosses.map((g) => `${s.label} ${g}`)),
    );
  }
  return byWord;
}

// ---------- output ----------

const escape = (s) => s.replace(/\\/g, '\\\\').replace(/\t/g, ' ').replace(/\r?\n/g, '\\n');

function write(name, byWord, source) {
  const rows = [];
  for (const [word, lines] of byWord) {
    if (!word || word.length > 200) continue;
    rows.push(`${escape(word)}\t${escape(lines.slice(0, MAX_LINES).join('\n'))}`);
  }
  rows.sort();
  const gz = gzipSync(rows.join('\n') + '\n', { level: 9 });
  writeFileSync(path.join(OUT_DIR, `${name}.tsv.gz`), gz);
  console.log(`${name}: ${rows.length} words, ${(gz.length / 1e6).toFixed(1)} MB`);
  return { file: `${name}.tsv.gz`, source, words: rows.length, sha256: createHash('sha256').update(gz).digest('hex') };
}

async function main() {
  mkdirSync(OUT_DIR, { recursive: true });
  const cedictVersion = JSON.parse(readFileSync(path.join(path.dirname(require.resolve('cedict-json')), 'package.json'), 'utf8')).version;
  const jmdict = await loadJmdict();
  const manifest = {
    'zh-en': write('zh-en', buildChinese(), `CC-CEDICT (cedict-json ${cedictVersion}), CC BY-SA 4.0`),
    'ja-en': write('ja-en', buildJapanese(jmdict), `JMdict ${jmdict.dictDate} (EDRDG), CC BY-SA 4.0`),
    'en-en': write('en-en', buildEnglish(), 'WordNet 3.1, Princeton University'),
  };
  writeFileSync(path.join(OUT_DIR, 'manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
}

if (import.meta.main ?? process.argv[1] === import.meta.filename) await main();

// Reads the page with LWT once the user switches it on for this site: words are tinted by learning level with the CSS
// Custom Highlight API, which leaves the page's DOM alone, and clicking one opens a card to look it up and set its level.
// Selecting several words (or "GPT-6", "U.S.") offers to join them into one term. A dock shows the page's word levels and
// how far it has been read, and switches read mode, in which words ignore clicks and selections.

(() => {
  const SHOWN_LEVELS = ['unknown', 'learning-1', 'learning-2', 'learning-3', 'learning-4', 'learning-5'];
  const LEVEL_KEYS = [
    ['unknown', '?', 'Unknown'],
    ['learning-1', '1', 'Learning 1'],
    ['learning-2', '2', 'Learning 2'],
    ['learning-3', '3', 'Learning 3'],
    ['learning-4', '4', 'Learning 4'],
    ['learning-5', '5', 'Learning 5'],
    ['well-known', 'w', 'Well known'],
    ['ignored', 'i', 'Ignored'],
  ];
  // The server takes at most 500 segments and 50,000 characters per request.
  const MAX_SEGMENTS = 500;
  const MAX_CHARS = 45_000;
  const SKIPPED = 'script, style, noscript, textarea, input, select, option, code, kbd, samp, svg, math, iframe';
  const INTERACTIVE = 'a[href], button, label, summary, select, [role="button"], [role="link"], [onclick]';

  let languageCode = null;
  let language = null;
  /** Term by key, as the server sent it; a key without one has no term yet. */
  const terms = new Map();
  /** Words found in each text node: { start, end, key, range }. */
  const nodes = new Map();
  const pending = new Set();
  let observer = null;
  let flushTimer = null;
  let running = Promise.resolve();
  let ui = null;
  let readMode = false;

  function api(path, method = 'GET', body) {
    return chrome.runtime.sendMessage({ type: 'api', path, method, body });
  }

  // --- Finding and annotating text ---

  function readable(text) {
    const parent = text.parentElement;
    if (!parent || !text.data.trim()) return false;
    if (ui && ui.host.contains(parent)) return false;
    return !parent.closest(SKIPPED) && !parent.isContentEditable;
  }

  function collect(root, into) {
    if (root.nodeType === Node.TEXT_NODE) {
      if (readable(root)) into.add(root);
      return;
    }
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, {
      acceptNode: (n) => (readable(n) ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_REJECT),
    });
    while (walker.nextNode()) into.add(walker.currentNode);
  }

  function batches(texts) {
    const result = [];
    let batch = [];
    let chars = 0;
    for (const text of texts) {
      // A node longer than a whole request is cut to fit; its words past that point stay unread.
      const length = Math.min(text.data.length, MAX_CHARS);
      if (batch.length && (batch.length >= MAX_SEGMENTS || chars + length > MAX_CHARS)) {
        result.push(batch);
        batch = [];
        chars = 0;
      }
      batch.push(text);
      chars += length;
    }
    if (batch.length) result.push(batch);
    return result;
  }

  async function annotate(texts) {
    const code = languageCode;
    for (const batch of batches(texts)) {
      const sent = batch.map((t) => t.data.slice(0, MAX_CHARS));
      const res = await api('/api/ext/annotate', 'POST', { languageCode: code, segments: sent });
      if (code !== languageCode) return;
      if (!res?.ok) {
        toast(res?.error ?? 'LWT could not read this page.');
        return;
      }
      for (const [key, term] of Object.entries(res.data.terms)) terms.set(key, term);
      batch.forEach((text, i) => {
        // Text that changed while it was being read gets read again by the observer.
        if (text.isConnected && text.data.slice(0, MAX_CHARS) === sent[i]) nodes.set(text, res.data.segments[i]);
      });
      paint();
    }
  }

  function enqueue(work) {
    running = running.then(work, work);
    return running;
  }

  function rangeOf(text, token) {
    if (!token.range) {
      token.range = new Range();
      token.range.setStart(text, token.start);
      token.range.setEnd(text, token.end);
    }
    return token.range;
  }

  function paint() {
    const highlights = Object.fromEntries(SHOWN_LEVELS.map((l) => [l, new Highlight()]));
    const counts = {};
    for (const [text, tokens] of nodes) {
      if (!text.isConnected) {
        nodes.delete(text);
        continue;
      }
      for (const token of tokens) {
        const level = terms.get(token.key)?.learningLevel ?? 'unknown';
        highlights[level]?.add(rangeOf(text, token));
        counts[level] = (counts[level] ?? 0) + 1;
      }
    }
    for (const level of SHOWN_LEVELS) CSS.highlights.set(`lwt-${level}`, highlights[level]);
    updateStats(counts);
  }

  /** Reads the page again, e.g. after a phrase was made or split; tokens are replaced as their results come back. */
  function refresh() {
    enqueue(() => annotate([...nodes.keys()].filter((t) => t.isConnected)));
  }

  function observe() {
    observer = new MutationObserver((mutations) => {
      for (const m of mutations) {
        if (m.type === 'characterData') {
          nodes.delete(m.target);
          if (readable(m.target)) pending.add(m.target);
        } else {
          for (const added of m.addedNodes) collect(added, pending);
        }
      }
      clearTimeout(flushTimer);
      flushTimer = setTimeout(() => {
        const texts = [...pending].filter((t) => t.isConnected && !nodes.has(t));
        pending.clear();
        if (texts.length) enqueue(() => annotate(texts));
        else paint();
      }, 800);
    });
    observer.observe(document.body, { childList: true, subtree: true, characterData: true });
  }

  async function enable(code) {
    if (code === languageCode) return;
    disable();
    languageCode = code;
    const { languages = [] } = await chrome.storage.local.get('languages');
    language = languages.find((l) => l.code === code) ?? { code };
    const texts = new Set();
    collect(document.body, texts);
    observe();
    showDock();
    enqueue(() => annotate([...texts]));
  }

  function disable() {
    languageCode = null;
    observer?.disconnect();
    observer = null;
    clearTimeout(flushTimer);
    pending.clear();
    nodes.clear();
    terms.clear();
    for (const level of SHOWN_LEVELS) CSS.highlights.delete(`lwt-${level}`);
    closeCard();
    ui?.dock?.remove();
    if (ui) ui.dock = null;
  }

  // --- The word card ---

  function el(tag, props = {}, children = []) {
    const node = Object.assign(document.createElement(tag), props);
    node.append(...children);
    return node;
  }

  const STYLES = `
    :host { all: initial; }
    .card, .toast {
      position: absolute; z-index: 2147483647; box-sizing: border-box; width: 300px; padding: 10px;
      font: 13px/1.4 system-ui, sans-serif; color: #1a1a1a; background: #fff;
      border: 1px solid #ddd; border-radius: 8px; box-shadow: 0 6px 24px rgba(0, 0, 0, 0.18);
    }
    .card:focus { outline: none; }
    .head { display: flex; align-items: center; gap: 6px; }
    .word { flex: 1; font-size: 16px; font-weight: 600; word-break: break-word; }
    .icon { border: none; background: none; cursor: pointer; font-size: 15px; padding: 2px 4px; color: inherit; }
    textarea {
      display: block; width: 100%; box-sizing: border-box; min-height: 56px; margin: 8px 0 4px; padding: 6px;
      font: inherit; color: inherit; background: transparent; border: 1px solid #ccc; border-radius: 4px; resize: vertical;
    }
    .hint { min-height: 1.4em; color: #777; font-size: 12px; }
    .levels { display: flex; gap: 4px; margin-top: 6px; }
    .levels button {
      flex: 1; padding: 4px 0; font: inherit; font-weight: 600; cursor: pointer; color: inherit;
      background: var(--c, #eee); border: 2px solid transparent; border-radius: 4px;
    }
    .levels button[aria-pressed='true'] { border-color: #1a1a1a; }
    .toast { position: fixed; right: 16px; bottom: 16px; width: auto; max-width: 320px; }
    .chip {
      position: absolute; z-index: 2147483647; padding: 4px 10px; cursor: pointer;
      font: 600 12px/1.4 system-ui, sans-serif; color: #fff; background: #1f2937;
      border: none; border-radius: 999px; box-shadow: 0 4px 14px rgba(0, 0, 0, 0.25);
    }
    .dock {
      position: fixed; left: 12px; bottom: 12px; z-index: 2147483646; box-sizing: border-box; width: 260px;
      padding: 8px; display: flex; flex-direction: column; gap: 6px;
      font: 12px/1.3 system-ui, sans-serif; color: #1a1a1a; background: rgba(255, 255, 255, 0.96);
      border: 1px solid #ddd; border-radius: 10px; box-shadow: 0 6px 24px rgba(0, 0, 0, 0.16);
    }
    .dock .row { display: flex; align-items: center; gap: 8px; }
    .mode {
      padding: 3px 10px; cursor: pointer; font: 600 12px/1.4 system-ui, sans-serif; color: inherit;
      background: transparent; border: 1px solid #ccc; border-radius: 999px;
    }
    .mode[aria-pressed='true'] { color: #fff; background: #1f2937; border-color: #1f2937; }
    .percent { margin-left: auto; color: #666; font-variant-numeric: tabular-nums; }
    .bar { display: flex; gap: 2px; height: 16px; font-size: 10px; line-height: 16px; font-weight: 600; }
    .bar > div { overflow: hidden; padding: 0 4px; white-space: nowrap; color: #1a1a1a; border-radius: 2px; }
    .bar > div:first-child { border-radius: 8px 2px 2px 8px; }
    .bar > div:last-child { border-radius: 2px 8px 8px 2px; }
    .bar > div:only-child { border-radius: 8px; }
    .progress { height: 4px; overflow: hidden; background: #e5e7eb; border-radius: 2px; }
    .progress > div { height: 100%; width: 0; background: #f97316; }
    @media (prefers-color-scheme: dark) {
      .card, .toast, .dock { color: #eee; background: #222; border-color: #444; }
      .levels button[aria-pressed='true'] { border-color: #eee; }
      .mode { border-color: #555; }
      .mode[aria-pressed='true'] { color: #111; background: #eee; border-color: #eee; }
      .percent { color: #aaa; }
      .progress { background: #444; }
    }
  `;
  const LEVEL_COLORS = {
    unknown: '#f27573',
    'learning-1': '#c786d3',
    'learning-2': '#4dabf5',
    'learning-3': '#33c9dc',
    'learning-4': '#a2cf6e',
    'learning-5': '#ffef62',
    'well-known': '#d9d9d9',
    ignored: '#9ca3af',
    learned: '#86d08a',
    learning: '#f5b1b0',
  };

  function ensureUi() {
    if (ui) return ui;
    const host = el('lwt-web-reader');
    const root = host.attachShadow({ mode: 'open' });
    // A constructed sheet, unlike a <style> element, isn't blocked by the page's Content-Security-Policy.
    const sheet = new CSSStyleSheet();
    sheet.replaceSync(STYLES);
    root.adoptedStyleSheets = [sheet];
    document.documentElement.append(host);
    ui = { host, root, card: null, chip: null, dock: null, toastTimer: null };
    return ui;
  }

  function toast(message) {
    const { root } = ensureUi();
    root.querySelector('.toast')?.remove();
    root.append(el('div', { className: 'toast', textContent: `LWT: ${message}` }));
    clearTimeout(ui.toastTimer);
    ui.toastTimer = setTimeout(() => root.querySelector('.toast')?.remove(), 5000);
  }

  function closeCard() {
    ui?.card?.remove();
    ui?.chip?.remove();
    if (ui) Object.assign(ui, { card: null, chip: null });
  }

  function speak(word) {
    const utterance = new SpeechSynthesisUtterance(word);
    if (language?.speakCode) utterance.lang = language.speakCode.replace('_', '-');
    speechSynthesis.cancel();
    speechSynthesis.speak(utterance);
  }

  /** The card for a word on the page: its term, if any, is updated or made from the word. */
  function openWordCard(text, token) {
    const term = terms.get(token.key);
    const code = languageCode;
    openCard({
      word: text.data.slice(token.start, token.end),
      rect: rangeOf(text, token).getBoundingClientRect(),
      term,
      async save(learningLevel, meaning) {
        const word = text.data.slice(token.start, token.end);
        const res = await api('/api/ext/terms', 'PUT', {
          ...(term ? { termId: term.id } : { languageCode: code, word }),
          learningLevel,
          meaning,
        });
        if (res?.ok && code === languageCode) {
          const { id, meaning: saved, learningLevel: level } = res.data;
          terms.set(token.key, { id, learningLevel: level, meaning: saved, phrase: term?.phrase ?? false });
          paint();
        }
        return res;
      },
      async split() {
        const res = await api('/api/ext/phrases/split', 'POST', { termId: term.id });
        if (res?.ok && code === languageCode) refresh();
        return res;
      },
    });
  }

  /** The card for selected text that isn't one term yet: saving joins it into one term everywhere. */
  function openPhraseCard(text, rect) {
    const code = languageCode;
    openCard({
      word: text.replace(/\s+/g, ' ').trim(),
      rect,
      term: null,
      hint: 'Saving joins these into one term, here and wherever they appear.',
      async save(learningLevel, meaning) {
        const res = await api('/api/ext/phrases', 'POST', { languageCode: code, text, learningLevel, meaning });
        if (res?.ok && code === languageCode) {
          getSelection().removeAllRanges();
          refresh();
        }
        return res;
      },
    });
  }

  function openCard({ word, rect, term, hint: hintText = '', save, split }) {
    const { root } = ensureUi();
    closeCard();
    const current = term?.learningLevel ?? 'unknown';
    const code = languageCode;

    const meaning = el('textarea', { value: term?.meaning ?? '', placeholder: 'Meaning' });
    const hint = el('div', { className: 'hint', textContent: hintText });
    const run = async (action) => {
      hint.textContent = 'Saving…';
      const res = await action();
      if (!res?.ok) {
        hint.textContent = res?.error ?? 'Could not save.';
        return;
      }
      closeCard();
    };
    const levels = LEVEL_KEYS.map(([level, label, title]) => {
      const button = el('button', { type: 'button', textContent: label.toUpperCase(), title: `${title} (${label})` });
      button.style.setProperty('--c', LEVEL_COLORS[level]);
      button.setAttribute('aria-pressed', String(Boolean(term) && level === current));
      button.addEventListener('click', () => run(() => save(level, meaning.value.trim())));
      return button;
    });
    meaning.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        run(() => save(current, meaning.value.trim()));
      }
    });

    const card = el('div', { className: 'card' }, [
      el('div', { className: 'head' }, [
        el('span', { className: 'word', textContent: word }),
        ...(term?.phrase && split
          ? [
              el('button', {
                className: 'icon',
                type: 'button',
                textContent: '✂',
                title: 'Split back into separate words, everywhere',
                onclick: () => run(split),
              }),
            ]
          : []),
        el('button', { className: 'icon', type: 'button', textContent: '🔊', title: 'Speak', onclick: () => speak(word) }),
        el('button', { className: 'icon', type: 'button', textContent: '✕', title: 'Close (Esc)', onclick: closeCard }),
      ]),
      meaning,
      hint,
      el('div', { className: 'levels' }, levels),
    ]);
    card.addEventListener('keydown', onCardKey);
    card.tabIndex = -1;
    root.append(card);
    ui.card = card;
    place(card, rect);
    // Focused so the level keys work right away.
    card.focus({ preventScroll: true });

    if (!term?.meaning && current !== 'ignored' && current !== 'well-known') {
      hint.textContent = 'Looking up…';
      api(`/api/ext/lookup?languageCode=${encodeURIComponent(code)}&word=${encodeURIComponent(word)}`).then((res) => {
        if (ui?.card !== card) return;
        const found = res?.ok && res.data.status === 'found';
        if (found && !meaning.value) meaning.value = res.data.meaning;
        const status = found
          ? 'From the dictionary.'
          : res?.ok && res.data.status === 'no-dictionary'
            ? 'Choose a dictionary language for this language in your LWT profile.'
            : res?.ok && res.data.status === 'missing'
              ? 'Not in the dictionary.'
              : 'The dictionary could not be reached.';
        hint.textContent = hintText ? `${status} ${hintText}` : status;
      });
    }
  }

  function place(card, rect) {
    if (!rect) return;
    const width = card.offsetWidth;
    const height = card.offsetHeight;
    const left = Math.max(8, Math.min(rect.left, innerWidth - width - 8));
    const below = rect.bottom + 6 + height <= innerHeight || rect.top < height + 6;
    const top = below ? rect.bottom + 6 : rect.top - height - 6;
    card.style.left = `${left + scrollX}px`;
    card.style.top = `${top + scrollY}px`;
  }

  function onCardKey(e) {
    if (e.key === 'Escape') {
      closeCard();
      return;
    }
    if (e.target instanceof HTMLTextAreaElement || e.ctrlKey || e.metaKey || e.altKey) return;
    const level = LEVEL_KEYS.find(([, key]) => key === e.key.toLowerCase());
    if (level) ui.card.querySelectorAll('.levels button')[LEVEL_KEYS.indexOf(level)].click();
  }

  // --- Clicking words ---

  function tokenAt(x, y) {
    let node;
    let offset;
    const position = document.caretPositionFromPoint?.(x, y);
    if (position) {
      node = position.offsetNode;
      offset = position.offset;
    } else {
      const range = document.caretRangeFromPoint?.(x, y);
      node = range?.startContainer;
      offset = range?.startOffset;
    }
    const tokens = node && nodes.get(node);
    if (!tokens) return null;
    // The caret lands between characters, so a click on a word's last letter can sit at its end.
    const token = tokens.find(
      (t) =>
        t.start <= offset &&
        offset <= t.end &&
        [...rangeOf(node, t).getClientRects()].some((r) => x >= r.left && x <= r.right && y >= r.top && y <= r.bottom),
    );
    return token ? { text: node, token } : null;
  }

  document.addEventListener(
    'click',
    (e) => {
      if (!languageCode || readMode || e.button !== 0 || e.ctrlKey || e.metaKey || e.shiftKey) return;
      if (ui && e.composedPath().includes(ui.host)) return;
      if (!getSelection().isCollapsed) return;
      const hit = tokenAt(e.clientX, e.clientY);
      if (!hit) {
        closeCard();
        return;
      }
      // Links and buttons keep working; Alt+click reads the word on them instead.
      if (e.target instanceof Element && e.target.closest(INTERACTIVE) && !e.altKey) return;
      e.preventDefault();
      e.stopPropagation();
      openWordCard(hit.text, hit.token);
    },
    true,
  );

  // --- Selecting words to join ---

  /** The token of an annotated text node around offset; atEnd looks for one ending there rather than starting. */
  function tokenAround(node, offset, atEnd) {
    return nodes.get(node)?.find((t) => (atEnd ? t.start < offset && offset <= t.end : t.start <= offset && offset < t.end));
  }

  function onSelectionDone() {
    const selection = getSelection();
    if (!languageCode || readMode || selection.isCollapsed || !selection.rangeCount) return;
    if (ui && (ui.host.contains(selection.anchorNode) || selection.anchorNode === ui.host)) return;
    const range = selection.getRangeAt(0).cloneRange();
    if (!nodes.has(range.startContainer) && !nodes.has(range.endContainer)) return;
    // Whole words: a selection that starts or ends inside one takes all of it.
    const first = tokenAround(range.startContainer, range.startOffset, false);
    const last = tokenAround(range.endContainer, range.endOffset, true);
    if (first) range.setStart(range.startContainer, first.start);
    if (last) range.setEnd(range.endContainer, last.end);
    const text = range.toString();
    if (!text.trim() || text.length > 300) return;
    if (first && first === last && range.startContainer === range.endContainer) {
      openWordCard(range.startContainer, first);
      return;
    }
    const { root } = ensureUi();
    closeCard();
    const rect = range.getBoundingClientRect();
    const chip = el('button', { className: 'chip', type: 'button', textContent: 'Make one term', title: text.trim() });
    // Keeps the page selection while clicking.
    chip.addEventListener('mousedown', (e) => e.preventDefault());
    chip.addEventListener('click', () => openPhraseCard(text, rect));
    root.append(chip);
    ui.chip = chip;
    chip.style.left = `${Math.max(8, Math.min(rect.right - chip.offsetWidth, innerWidth - chip.offsetWidth - 8)) + scrollX}px`;
    chip.style.top = `${rect.bottom + 6 + scrollY}px`;
  }

  document.addEventListener('mouseup', (e) => {
    if (e.button !== 0 || (ui && e.composedPath().includes(ui.host))) return;
    // The selection is final once this event is over.
    setTimeout(onSelectionDone, 0);
  });

  // --- The dock: page statistics, reading progress and read mode ---

  const SHORT_LABELS = { unknown: 'UK', 'learning-1': 'L1', 'learning-2': 'L2', 'learning-3': 'L3', 'learning-4': 'L4', 'learning-5': 'L5' };
  const LONG_LABELS = Object.fromEntries(LEVEL_KEYS.map(([level, , title]) => [level, title]));

  function showDock() {
    const { root } = ensureUi();
    if (ui.dock) return;
    const mode = el('button', { className: 'mode', type: 'button' });
    mode.addEventListener('click', () => setReadMode(!readMode));
    const percent = el('span', { className: 'percent' });
    const words = el('div', { className: 'bar', role: 'img' });
    const byLevel = el('div', { className: 'bar', role: 'img' });
    const progress = el('div', { className: 'progress', title: 'How far down the page you are' }, [el('div')]);
    ui.dock = el('div', { className: 'dock' }, [el('div', { className: 'row' }, [mode, percent]), words, byLevel, progress]);
    Object.assign(ui.dock, { mode, percent, words, byLevel, progress: progress.firstChild });
    root.append(ui.dock);
    renderMode();
    updateProgress();
  }

  function renderMode() {
    const mode = ui?.dock?.mode;
    if (!mode) return;
    mode.textContent = readMode ? '📖 Reading' : '👆 Learning';
    mode.setAttribute('aria-pressed', String(readMode));
    mode.title = readMode
      ? 'Read mode: words ignore clicks and selections. Click to learn again.'
      : 'Turn on read mode: words ignore clicks and selections';
  }

  function setReadMode(on) {
    readMode = on;
    if (on) closeCard();
    renderMode();
    chrome.storage.local.set({ readMode: on });
  }

  function fillBar(bar, segments, label) {
    const total = segments.reduce((n, s) => n + s.value, 0);
    bar.setAttribute('aria-label', `${label}: ${segments.map((s) => `${s.title} ${s.value}`).join(', ')}`);
    bar.replaceChildren(
      ...segments
        .filter((s) => s.value > 0)
        .map((s) => {
          const share = Math.round((s.value * 100) / total);
          const part = el('div', { textContent: share >= 12 ? `${s.label} ${share}%` : '', title: `${s.title}: ${s.value} (${share}%)` });
          part.style.flexGrow = String(s.value);
          part.style.background = LEVEL_COLORS[s.key];
          return part;
        }),
    );
  }

  /** Word occurrences on the page by level, as the reader's statistics bars show a text's. */
  function updateStats(counts) {
    const dock = ui?.dock;
    if (!dock) return;
    const known = counts['well-known'] ?? 0;
    const learning = SHOWN_LEVELS.reduce((n, l) => n + (counts[l] ?? 0), 0);
    fillBar(
      dock.words,
      [
        { key: 'learned', label: 'Learned', title: 'Learned', value: known },
        { key: 'learning', label: 'Learning', title: 'Learning', value: learning },
      ],
      'Words',
    );
    fillBar(
      dock.byLevel,
      SHOWN_LEVELS.map((l) => ({ key: l, label: SHORT_LABELS[l], title: LONG_LABELS[l], value: counts[l] ?? 0 })),
      'Learning words by level',
    );
  }

  let progressFrame = 0;
  function updateProgress() {
    progressFrame = 0;
    const dock = ui?.dock;
    if (!dock) return;
    const scrollable = document.documentElement.scrollHeight - innerHeight;
    const share = scrollable > 0 ? Math.min(1, Math.max(0, scrollY / scrollable)) : 1;
    dock.progress.style.width = `${share * 100}%`;
    dock.percent.textContent = `${Math.round(share * 100)}% read`;
  }

  addEventListener('scroll', () => (progressFrame ||= requestAnimationFrame(updateProgress)), { passive: true });
  addEventListener('resize', () => (progressFrame ||= requestAnimationFrame(updateProgress)), { passive: true });

  document.addEventListener(
    'keydown',
    (e) => {
      if ((ui?.card || ui?.chip) && e.key === 'Escape') closeCard();
    },
    true,
  );

  // --- Wiring ---

  chrome.runtime.onMessage.addListener((message, _sender, reply) => {
    if (message.type === 'status') {
      reply({ pageLanguage: document.documentElement.lang.slice(0, 2).toLowerCase() || null, languageCode });
    } else if (message.type === 'enable') {
      enable(message.languageCode);
      reply({ ok: true });
    } else if (message.type === 'disable') {
      disable();
      reply({ ok: true });
    }
  });

  chrome.storage.onChanged.addListener((changes, area) => {
    if (area !== 'local' || !changes.readMode) return;
    readMode = Boolean(changes.readMode.newValue);
    if (readMode) closeCard();
    renderMode();
  });

  chrome.storage.local.get(['sites', 'token', 'readMode']).then(({ sites = {}, token, readMode: savedReadMode }) => {
    readMode = Boolean(savedReadMode);
    const code = sites[location.hostname];
    if (code && token && document.body) enable(code);
  });
})();

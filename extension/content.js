// Reads the page with LWT once the user switches it on for this site: words are tinted by learning level with the CSS
// Custom Highlight API, which leaves the page's DOM alone, and clicking one opens a card to look it up and set its level.

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
    for (const [text, tokens] of nodes) {
      if (!text.isConnected) {
        nodes.delete(text);
        continue;
      }
      for (const token of tokens) {
        const level = terms.get(token.key)?.learningLevel ?? 'unknown';
        highlights[level]?.add(rangeOf(text, token));
      }
    }
    for (const level of SHOWN_LEVELS) CSS.highlights.set(`lwt-${level}`, highlights[level]);
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
    @media (prefers-color-scheme: dark) {
      .card, .toast { color: #eee; background: #222; border-color: #444; }
      .levels button[aria-pressed='true'] { border-color: #eee; }
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
    ui = { host, root, card: null, toastTimer: null };
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
    if (ui) ui.card = null;
  }

  function speak(word) {
    const utterance = new SpeechSynthesisUtterance(word);
    if (language?.speakCode) utterance.lang = language.speakCode.replace('_', '-');
    speechSynthesis.cancel();
    speechSynthesis.speak(utterance);
  }

  function openCard(text, token) {
    const { root } = ensureUi();
    closeCard();
    const word = text.data.slice(token.start, token.end);
    const term = terms.get(token.key);
    const current = term?.learningLevel ?? 'unknown';
    const code = languageCode;

    const meaning = el('textarea', { value: term?.meaning ?? '', placeholder: 'Meaning' });
    const hint = el('div', { className: 'hint' });
    const save = async (learningLevel) => {
      hint.textContent = 'Saving…';
      const res = await api('/api/ext/terms', 'PUT', {
        ...(term ? { termId: term.id } : { languageCode: code, word }),
        learningLevel,
        meaning: meaning.value.trim(),
      });
      if (!res?.ok) {
        hint.textContent = res?.error ?? 'Could not save.';
        return;
      }
      if (code !== languageCode) return;
      const { id, meaning: saved } = res.data;
      terms.set(token.key, { id, learningLevel: res.data.learningLevel, meaning: saved });
      paint();
      closeCard();
    };
    const levels = LEVEL_KEYS.map(([level, label, title]) => {
      const button = el('button', { type: 'button', textContent: label.toUpperCase(), title: `${title} (${label})` });
      button.style.setProperty('--c', LEVEL_COLORS[level]);
      button.setAttribute('aria-pressed', String(level === current));
      button.addEventListener('click', () => save(level));
      return button;
    });
    meaning.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        save(current);
      }
    });

    const card = el('div', { className: 'card' }, [
      el('div', { className: 'head' }, [
        el('span', { className: 'word', textContent: word }),
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
    place(card, rangeOf(text, token).getBoundingClientRect());
    // Focused so the level keys work right away.
    card.focus({ preventScroll: true });

    if (!term?.meaning && current !== 'ignored' && current !== 'well-known') {
      hint.textContent = 'Looking up…';
      api(`/api/ext/lookup?languageCode=${encodeURIComponent(code)}&word=${encodeURIComponent(word)}`).then((res) => {
        if (ui?.card !== card) return;
        const found = res?.ok && res.data.status === 'found';
        if (found && !meaning.value) meaning.value = res.data.meaning;
        hint.textContent = found
          ? 'From the dictionary.'
          : res?.ok && res.data.status === 'no-dictionary'
            ? 'Choose a dictionary language for this language in your LWT profile.'
            : res?.ok && res.data.status === 'missing'
              ? 'Not in the dictionary.'
              : 'The dictionary could not be reached.';
      });
    }
  }

  function place(card, rect) {
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
      if (!languageCode || e.button !== 0 || e.ctrlKey || e.metaKey || e.shiftKey) return;
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
      openCard(hit.text, hit.token);
    },
    true,
  );

  document.addEventListener(
    'keydown',
    (e) => {
      if (ui?.card && e.key === 'Escape') closeCard();
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

  chrome.storage.local.get(['sites', 'token']).then(({ sites = {}, token }) => {
    const code = sites[location.hostname];
    if (code && token && document.body) enable(code);
  });
})();

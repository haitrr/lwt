// Sign-in, and switching LWT on or off for the current tab's site in a language.

const $ = (id) => document.getElementById(id);

function showError(message) {
  $('error').textContent = message ?? '';
}

async function currentTab() {
  const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
  return tab;
}

function hostOf(tab) {
  try {
    const url = new URL(tab.url);
    return /^https?:$/.test(url.protocol) ? url.hostname : null;
  } catch {
    return null;
  }
}

async function render() {
  const { server, token, userName, languages = [], sites = {} } = await chrome.storage.local.get([
    'server',
    'token',
    'userName',
    'languages',
    'sites',
  ]);
  $('login').hidden = Boolean(token);
  $('site').hidden = !token;
  if (!token) {
    $('login').server.value = server ?? '';
    return;
  }
  $('user').textContent = userName;
  $('server').textContent = server;

  const tab = await currentTab();
  const host = hostOf(tab);
  $('host').textContent = host ?? 'This page can’t be read with LWT.';
  $('enabled').disabled = !host;
  $('language').replaceChildren(...languages.map((l) => new Option(l.name, l.code)));

  // The page's own language is the best guess until the user picks one for the site.
  let pageLanguage = null;
  if (host) {
    pageLanguage = await chrome.tabs.sendMessage(tab.id, { type: 'status' }).then((s) => s?.pageLanguage, () => null);
  }
  const guess = languages.find((l) => l.code === pageLanguage)?.code;
  $('language').value = sites[host] ?? guess ?? languages[0]?.code ?? '';
  $('enabled').checked = Boolean(host && sites[host]);
}

async function updateSite() {
  const tab = await currentTab();
  const host = hostOf(tab);
  if (!host) return;
  const { sites = {} } = await chrome.storage.local.get('sites');
  const languageCode = $('language').value;
  if ($('enabled').checked) sites[host] = languageCode;
  else delete sites[host];
  await chrome.storage.local.set({ sites });
  // Pages opened before the extension was installed have no content script; they pick it up on reload.
  await chrome.tabs
    .sendMessage(tab.id, $('enabled').checked ? { type: 'enable', languageCode } : { type: 'disable' })
    .catch(() => showError('Reload the page to start reading it.'));
}

$('login').addEventListener('submit', async (e) => {
  e.preventDefault();
  showError();
  const form = e.target;
  const button = form.querySelector('button');
  button.disabled = true;
  const result = await chrome.runtime.sendMessage({
    type: 'login',
    server: form.server.value,
    userName: form.userName.value,
    password: form.password.value,
  });
  button.disabled = false;
  if (!result.ok) return showError(result.error);
  form.password.value = '';
  render();
});

$('enabled').addEventListener('change', updateSite);
$('language').addEventListener('change', () => $('enabled').checked && updateSite());

$('logout').addEventListener('click', async () => {
  await chrome.storage.local.remove(['token', 'userName', 'languages']);
  render();
});

render();

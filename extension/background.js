// Talks to the LWT server for the popup and the content scripts. Requests from here are not subject to the page's
// CORS rules (the extension has host permissions), and the token never reaches the pages being read.

async function settings() {
  return chrome.storage.local.get(['server', 'token']);
}

async function api({ method = 'GET', path, body }) {
  const { server, token } = await settings();
  if (!server) return { ok: false, status: 0, error: 'Set your LWT server in the extension popup.' };
  let res;
  try {
    res = await fetch(server + path, {
      method,
      headers: {
        ...(body ? { 'Content-Type': 'application/json' } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: body ? JSON.stringify(body) : undefined,
    });
  } catch {
    return { ok: false, status: 0, error: `Can't reach ${server}.` };
  }
  const data = await res.json().catch(() => null);
  if (res.status === 401 && token) await chrome.storage.local.remove(['token', 'userName', 'languages']);
  if (res.status === 404 && !data?.error) {
    return { ok: false, status: 404, error: `${server} has no extension API: is it LWT, updated to a version with the extension?` };
  }
  if (!res.ok) return { ok: false, status: res.status, error: data?.error ?? `LWT answered ${res.status}.` };
  return { ok: true, status: res.status, data };
}

async function login({ server, userName, password }) {
  server = server.trim().replace(/\/+$/, '');
  if (!/^https?:\/\/[^/]+/.test(server)) return { ok: false, error: 'The server must be an http(s) address.' };
  await chrome.storage.local.set({ server });
  await chrome.storage.local.remove('token');
  const signedIn = await api({ method: 'POST', path: '/api/ext/login', body: { userName, password } });
  if (!signedIn.ok) return signedIn;
  await chrome.storage.local.set({ token: signedIn.data.token, userName: signedIn.data.userName });
  const me = await api({ path: '/api/ext/me' });
  if (me.ok) await chrome.storage.local.set({ languages: me.data.languages });
  return me;
}

chrome.runtime.onMessage.addListener((message, _sender, reply) => {
  const run = message.type === 'login' ? login(message) : message.type === 'api' ? api(message) : null;
  if (!run) return false;
  run.then(reply, (e) => reply({ ok: false, status: 0, error: String(e) }));
  return true;
});

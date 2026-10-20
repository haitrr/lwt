# Migration from the CRA app and the .NET API

Everything the old app did is ported. The old code is on `master` here (API and Japanese segmenter) and in haitrr/lwt.web (frontend).

| Old | New |
| --- | --- |
| `/login`, `/register` | Server actions; JWT in an httpOnly cookie instead of localStorage |
| `/` last read text | `src/app/(app)/page.tsx` |
| `/text` list, filter, create, edit, delete | `src/app/(app)/text` (create/edit are pages instead of modals) |
| `/text/read/:id` reader | `src/app/(app)/text/read/[textId]`: colored terms, hover card with Better/Worse, edit panel, bookmark, speech, dictionary suggestions, infinite scroll |
| `/statistics` (Victory pies) | Bar lists, no chart library |
| `/profile` dictionary languages | Same, plus change password (the API had it, the UI didn't) |
| `/api/*` (.NET) | `src/server/*`, called directly from pages and server functions; there is no public REST API now |
| Japanese segmenter service (Python/MeCab) | kuromoji in-process |

## Data

No migration: the same MySQL tables and columns are used. Existing logins, texts, terms and settings keep working, and
the `logs` table keeps being written in the same format (it also drives "last read").

Behavior changes worth knowing:
- Emoji are kept as one token. The old splitter cut them into two broken characters.
- A word that matches an existing term only under MySQL's case/accent-insensitive collation (e.g. `ｱ` vs `ア`) now links to
  that term. The old processor failed on the unique index and retried that text forever.
- Line breaks typed into the text form are stored as `\n`, like before.
- Vietnamese is listed but still can't be split, as before.

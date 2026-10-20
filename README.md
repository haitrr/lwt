# lwt

[LWT](https://sourceforge.net/projects/lwt/)-style reading app: import texts, read them with every word colored by how well
you know it, and track progress. Next.js (App Router, TypeScript) with MySQL; it replaces both the old .NET API (this repo's `master`) and the
React app in [haitrr/lwt.web](https://github.com/haitrr/lwt.web), and works on the API's existing database as is.

## Getting started

```shell
cp .env.example .env.local   # set DATABASE_URL and LWT_JWT_SECRET
npm install
npm run dev
```

The app expects the schema the .NET API created with its EF Core migrations, saved in [db/schema.sql](db/schema.sql).
Load that file into a fresh database (`docker compose` does it for you), or point the app at the existing one.

| Variable | |
| --- | --- |
| `DATABASE_URL` | MySQL 8 connection string, e.g. `mysql://user:pass@host:3306/lwt` |
| `LWT_JWT_SECRET` | Secret for session tokens. Use the .NET API's `AppSettings:Secret` to keep its tokens valid. |
| `LWT_DICTIONARY_API_ROOT` | Optional. Dictionary service for meaning suggestions (`GET {root}/dictionary/{word}?fromLang=&toLang=`). |

`npm test` runs the unit tests, `npm run lint` and `npm run typecheck` the static checks.

## How it works

- `src/server` is the backend: Drizzle on the existing tables (`db/schema.ts`), ASP.NET Identity compatible password
  hashes (`auth/password.ts`), and the text, term and user logic ported from the .NET services.
- New and edited texts are split into words in the background (`server/processing.ts`, started from
  `src/instrumentation.ts`), 1000 words per step, like the .NET `TextTermProcessor`. Chinese uses a port of Jieba.Net's
  segmenter with the same dictionary, Japanese uses kuromoji (IPADIC, as the old MeCab service did). The tests check
  both against output recorded from the old API.
- Pages are server components; mutations are server functions in `src/lib`.

## Deploying

`docker compose up` builds the app (Next.js standalone output) and starts it with MySQL. To move from the .NET API, point
`DATABASE_URL` at its database and stop the old API and the Japanese segmenter service: both apps would otherwise split
texts at the same time. See [MIGRATION.md](MIGRATION.md).

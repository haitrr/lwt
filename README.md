# lwt

[LWT](https://sourceforge.net/projects/lwt/)-style reading app: import texts, read them with every word colored by how well
you know it, and track progress. Next.js (App Router, TypeScript) with Postgres; it replaces both the old .NET API on MySQL
(this repo's `master`) and the React app in [haitrr/lwt.web](https://github.com/haitrr/lwt.web).

![Reading a text: hover a word to see its meaning and move it up a level, click it to edit](docs/screenshots/demo.gif)

| | |
| --- | --- |
| ![Reader with an English text](docs/screenshots/reader-en.png) | ![Reader with a Chinese text](docs/screenshots/reader-zh.png) |
| ![Texts list](docs/screenshots/texts.png) | ![Statistics](docs/screenshots/statistics.png) |

## Getting started

```shell
cp .env.example .env.local   # set DATABASE_URL and LWT_JWT_SECRET
npm install
npm run dev
```

The app creates and updates its tables when it starts (Drizzle migrations in `drizzle/`). After changing
`src/server/db/schema.ts`, run `npx drizzle-kit generate` to add a migration.

| Variable | |
| --- | --- |
| `DATABASE_URL` | Postgres connection string (tested with 17), e.g. `postgres://user:pass@host:5432/lwt` |
| `LWT_JWT_SECRET` | Secret for session tokens. Use the .NET API's `AppSettings:Secret` to keep its tokens valid. |
| `LWT_DICTIONARY_API_ROOT` | Optional. Dictionary service for meaning suggestions (`GET {root}/dictionary/{word}?fromLang=&toLang=`). |

`npm test` runs the unit tests, `npm run lint` and `npm run typecheck` the static checks.

## How it works

- `src/server` is the backend: Drizzle on Postgres (`db/schema.ts`), ASP.NET Identity compatible password hashes
  (`auth/password.ts`), and the text, term and user logic ported from the .NET services. Terms are compared ignoring
  case, accents and width, like the old MySQL collation did.
- New and edited texts are split into words in the background (`server/processing.ts`, started from
  `src/instrumentation.ts`), 1000 words per step, like the .NET `TextTermProcessor`. Chinese uses a port of Jieba.Net's
  segmenter with the same dictionary, Japanese uses kuromoji (IPADIC, as the old MeCab service did). The tests check
  both against output recorded from the old API.
- Pages are server components; mutations are server functions in `src/lib`.

## Deploying

`docker compose up` builds the app (Next.js standalone output) and starts it with Postgres. Set `POSTGRES_PASSWORD` and
`LWT_JWT_SECRET` in the environment or a `.env` file.

### Moving from MySQL

Stop the old .NET API first, so nothing writes to MySQL during the copy. Then copy its data into an empty Postgres
database:

```shell
MYSQL_URL=mysql://user:pass@host:3306/lwt DATABASE_URL=postgres://user:pass@host:5432/lwt npm run import-mysql
```

The script creates the schema, copies users (passwords keep working), texts, terms, text splits, settings and logs with
their ids, and only reads from MySQL. A few terms that MySQL kept apart but Postgres treats as the same word (`don't` and
`don’t`) are merged into the older one, and the script prints each merge. See [MIGRATION.md](MIGRATION.md).

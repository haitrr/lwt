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
npm run build-dictionary     # optional: the built-in dictionaries, see below
npm run dev
```

The app creates and updates its tables when it starts (Drizzle migrations in `drizzle/`). After changing
`src/server/db/schema.ts`, run `npx drizzle-kit generate` to add a migration.

| Variable | |
| --- | --- |
| `DATABASE_URL` | Postgres connection string (tested with 17), e.g. `postgres://user:pass@host:5432/lwt` |
| `LWT_JWT_SECRET` | Secret for session tokens. Use the .NET API's `AppSettings:Secret` to keep its tokens valid. |

`npm test` runs the unit tests, `npm run lint` and `npm run typecheck` the static checks.

## How it works

- `src/server` is the backend: Drizzle on Postgres (`db/schema.ts`), ASP.NET Identity compatible password hashes
  (`auth/password.ts`), and the text, term and user logic ported from the .NET services. Terms are compared ignoring
  case, accents and width, like the old MySQL collation did.
- New and edited texts are split into words in the background (`server/processing.ts`, started from
  `src/instrumentation.ts`), 1000 words per step, like the .NET `TextTermProcessor`. Chinese uses a port of Jieba.Net's
  segmenter with the same dictionary, Japanese uses kuromoji (IPADIC, as the old MeCab service did). The tests check
  both against output recorded from the old API.
- Meaning suggestions in the reader come from dictionaries built into the app, so they work offline.
  `npm run build-dictionary` (run by the Dockerfile) turns open dictionary data into `dictionary/*.tsv.gz`, and the
  app loads any file that changed into the `dictionary_entries` table when it starts. Lookups try the word as written,
  then its dictionary form (`walked` → `walk`, `食べ` → `食べる`).

  | From → to | Data | License |
  | --- | --- | --- |
  | Chinese → English | [CC-CEDICT](https://www.mdbg.net/chinese/dictionary?page=cc-cedict) (via the `cedict-json` package) | CC BY-SA 4.0 |
  | Japanese → English | [JMdict](https://www.edrdg.org/wiki/index.php/JMdict-EDICT_Dictionary_Project) by the EDRDG (via a pinned [jmdict-simplified](https://github.com/scriptin/jmdict-simplified) release) | CC BY-SA 4.0 |
  | English → English | [WordNet 3.1](https://wordnet.princeton.edu), Princeton University (via the `wordnet-db` package) | WordNet license |

  Other language pairs get no suggestions. To update JMdict, change `JMDICT_TAG` and `JMDICT_SHA256` in
  `scripts/build-dictionary.mjs`; `npm update cedict-json` updates CC-CEDICT.
- Pages are server components; mutations are server functions in `src/lib`.
- The UI uses [shadcn/ui](https://ui.shadcn.com) components (`src/components/ui`, add more with `npx shadcn add <name>`) on
  Tailwind CSS v4, with light, dark and system themes.

## Installing with Docker

You need Docker with the Compose plugin (`docker compose version`). Nothing is built on the host: every push to master
publishes the app image to `ghcr.io/haitrr/lwt` as `latest` and `sha-<commit>` (`.github/workflows/docker.yml`), and
the compose file pulls it with Postgres 17 and the backup service. The built-in dictionaries are inside the image.

1. Download the compose file into an empty folder:

   ```shell
   mkdir lwt && cd lwt
   curl -fsSL -o compose.yaml https://raw.githubusercontent.com/haitrr/lwt/master/docker-compose.prod.yml
   ```

2. Make a backup key on your own machine (not the server). The backup service will not start without its public half:

   ```shell
   age-keygen -o lwt-backup.key        # keep this file offline; it is the only way to read a backup
   grep 'public key' lwt-backup.key    # the age1… value for BACKUP_AGE_RECIPIENT
   ```

3. Create `.env` next to `compose.yaml`:

   ```shell
   POSTGRES_PASSWORD=<a long random password>
   LWT_JWT_SECRET=<a long random string, e.g. from: openssl rand -base64 48>
   BACKUP_AGE_RECIPIENT=age1...
   # LWT_IMAGE_TAG=sha-<commit>        # optional: pin a build instead of latest
   ```

   Moving from the old .NET API? Use its `AppSettings:Secret` as `LWT_JWT_SECRET` so existing sessions stay signed in.

4. Point the backups at your storage. The `postgres-backups` volume at the bottom of `compose.yaml` is an NFS mount of
   `ugnas.lc:/volume2/backups/lwt`; change `addr=` and `device:` to your NAS, or replace it with a plain local volume
   (`postgres-backups: {}`) to keep the dumps on the host.

5. Start it, and open `http://<host>:3000` to register an account (or bring over the old data first, see
   [Moving from MySQL](#moving-from-mysql)). The app creates its tables on first start.

   ```shell
   docker compose up -d
   ```

To update, `docker compose pull && docker compose up -d`; to roll back, set `LWT_IMAGE_TAG=sha-<commit>` in `.env` and
run the same. `docker compose logs -f web` shows the app's logs.

On the server that has this repository checked out, `./deploy.sh` does the update with a backup first: it runs
`./backup.sh`, pulls the images and starts `docker-compose.prod.yml`. `docker-compose.yaml` is the same stack built from
the checkout (`COMPOSE_FILE=docker-compose.yaml docker compose up -d --build`).

### Backups

The `postgres-backup` service ([haitrr/pg-backup](https://github.com/haitrr/pg-backup)) dumps the database every hour
into the `postgres-backups` volume, an NFS mount of `ugnas.lc:/volume2/backups/lwt`. Each dump is `pg_dump | gzip |
age`, so nothing reaches the NAS unencrypted. Hourly dumps are kept for 48 hours, with daily, weekly and monthly copies
kept for 7 days, 4 weeks and 6 months. `./deploy.sh` also runs `./backup.sh` before each deploy, which keeps the last 20
dumps in `deployments/`.

The key pair is the one from step 2 of the install; `BACKUP_AGE_RECIPIENT` is its public half.

To restore, run `./restore.sh` with no arguments to list the dumps, then pass one with the private key. It asks you to
type the database name, since it replaces the database's contents, and runs in one transaction, so a failed restore
changes nothing:

```shell
./restore.sh /backups/daily/20261020.sql.gz.age ~/lwt-backup.key
```

Installed from the compose file alone, fetch the script next to `compose.yaml` and point it there:

```shell
curl -fsSLO https://raw.githubusercontent.com/haitrr/lwt/master/restore.sh && chmod +x restore.sh
COMPOSE_FILE=compose.yaml ./restore.sh
```

### Moving from MySQL

Stop the old .NET API first, so nothing writes to MySQL during the copy. Then copy its data into the new, still empty
Postgres database with the import script that ships in the image, and start the rest:

```shell
docker compose run --rm -e MYSQL_URL=mysql://user:pass@mysql-host:3306/lwt web node scripts/import-mysql.mjs
docker compose up -d
```

`mysql-host` must be reachable from inside the container: a hostname or IP on your network (not `localhost`). Outside
Docker it is `MYSQL_URL=... DATABASE_URL=... npm run import-mysql` from a checkout.

The script creates the schema, copies users (passwords keep working), texts, terms, text splits, settings and logs with
their ids, and only reads from MySQL. It refuses to run if the Postgres database already has users. A few terms that
MySQL kept apart but Postgres treats as the same word (`don't` and `don’t`) are merged into the older one, and the
script prints each merge. See [MIGRATION.md](MIGRATION.md).

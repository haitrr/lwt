#!/bin/bash
# One encrypted dump: pg_dump | gzip | age, never touching disk in between.
set -euo pipefail

DEST=${1:?usage: dump.sh <destination-dir> <filename-prefix> [keep]}
PREFIX=${2:?usage: dump.sh <destination-dir> <filename-prefix> [keep]}
KEEP=${3:-20}

# Refuse rather than fall back to plaintext.
: "${BACKUP_AGE_RECIPIENT:?BACKUP_AGE_RECIPIENT is unset, see Backups in README.md}"

mkdir -p "$DEST"
FINAL="$DEST/$PREFIX-$(date '+%Y%m%d-%H%M%S').sql.gz.age"

# Written under .part and renamed only once pg_dump, gzip and age have all
# succeeded, so a dump killed halfway never looks like a backup.
PART="$FINAL.part"
trap 'rm -f "$PART"' EXIT

# --clean --if-exists lets ./restore.sh load a dump over the live database.
PGPASSWORD="$POSTGRES_PASSWORD" pg_dump --clean --if-exists \
  -h "${POSTGRES_HOST:-postgres}" -U "$POSTGRES_USER" "$POSTGRES_DB" \
  | gzip \
  | age -r "$BACKUP_AGE_RECIPIENT" > "$PART"

mv "$PART" "$FINAL"
trap - EXIT

# Newest first; everything past the Nth goes.
ls -t "$DEST/$PREFIX"-*.sql.gz.age 2>/dev/null | tail -n +$((KEEP + 1)) | xargs -r rm --

echo "$FINAL"

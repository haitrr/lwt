#!/bin/bash
# Restore an encrypted dump into the compose stack's database.
#
#   ./restore.sh /backups/daily/20261020.sql.gz.age ~/lwt-backup.key
#
# The identity (private key) is passed in and never kept on the server. Give it
# as the second argument or in BACKUP_AGE_IDENTITY. The dump is read from the
# host if the path exists there, otherwise from inside the backup container,
# where the /backups volume lives.
set -euo pipefail

COMPOSE="docker compose"
SRC=${1:-}
IDENTITY=${2:-${BACKUP_AGE_IDENTITY:-}}

if [ -z "$SRC" ] || [ -z "$IDENTITY" ]; then
  echo "usage: ./restore.sh <dump.sql.gz.age> [identity-file]" >&2
  echo "       (the identity may also come from BACKUP_AGE_IDENTITY)" >&2
  echo "" >&2
  echo "available dumps:" >&2
  $COMPOSE exec -T postgres-backup sh -c 'ls -t /backups/*/*.sql.gz.age 2>/dev/null | head -20' >&2 || true
  exit 1
fi

[ -r "$IDENTITY" ] || { echo "Cannot read identity file: $IDENTITY" >&2; exit 1; }

DB=$($COMPOSE exec -T postgres printenv POSTGRES_DB </dev/null | tr -d '\r')
if [ "${RESTORE_ASSUME_YES:-}" != "1" ]; then
  echo "This replaces the contents of database '$DB' on this host."
  printf "Type the database name to confirm: "
  read -r CONFIRM
  [ "$CONFIRM" = "$DB" ] || { echo "Aborted."; exit 1; }
fi

# Decrypt in a throwaway container off the backup image, so the host needs no
# age and the long-lived sidecar never sees the private key.
IMAGE=$($COMPOSE config --images postgres-backup | grep postgres-backup | head -1)

if [ -f "$SRC" ]; then cat "$SRC"; else $COMPOSE exec -T postgres-backup cat "$SRC"; fi \
  | docker run --rm -i -v "$(realpath "$IDENTITY")":/run/id.key:ro \
      --entrypoint age "$IMAGE" -d -i /run/id.key \
  | gunzip \
  | $COMPOSE exec -T postgres sh -c \
      'PGPASSWORD=$POSTGRES_PASSWORD psql -q -v ON_ERROR_STOP=1 -U $POSTGRES_USER -d $POSTGRES_DB' >/dev/null

echo "Restored $SRC into '$DB'."

#!/bin/bash
# Restore an encrypted dump into the compose stack's database.
#
#   ./restore.sh /backups/daily/20261020.sql.gz.age ~/lwt-backup.key
#
# The identity (private key) is passed in and never kept on the server. Give it
# as the second argument or in BACKUP_AGE_IDENTITY. A dump path that exists on
# the host is mounted in; otherwise it is read from the /backups volume. The
# restore runs in a throwaway container, so the long-lived backup service never
# sees the key.
set -euo pipefail

SRC=${1:-}
IDENTITY=${2:-${BACKUP_AGE_IDENTITY:-}}

if [ -z "$SRC" ] || [ -z "$IDENTITY" ]; then
  echo "usage: ./restore.sh <dump.sql.gz.age> [identity-file]" >&2
  echo "       (the identity may also come from BACKUP_AGE_IDENTITY)" >&2
  echo "" >&2
  echo "available dumps:" >&2
  docker compose exec -T postgres-backup pg-backup list </dev/null >&2 || true
  exit 1
fi

[ -r "$IDENTITY" ] || { echo "Cannot read identity file: $IDENTITY" >&2; exit 1; }

MOUNTS=(-v "$(realpath "$IDENTITY")":/run/age.key:ro)
if [ -f "$SRC" ]; then
  MOUNTS+=(-v "$(realpath "$SRC")":/run/dump.sql.gz.age:ro)
  SRC=/run/dump.sql.gz.age
fi

docker compose run --rm "${MOUNTS[@]}" -e RESTORE_ASSUME_YES postgres-backup pg-backup restore "$SRC"

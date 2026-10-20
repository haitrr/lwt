#!/bin/bash
# Hourly dumps with grandfather-father-son retention. Each daily, weekly and
# monthly copy is a hard link to an hourly dump, so the longer tiers cost no
# extra bytes.
set -euo pipefail

# Checked at start, so a missing key is a container that fails to come up rather
# than one that looks healthy and writes nothing.
: "${BACKUP_AGE_RECIPIENT:?BACKUP_AGE_RECIPIENT is unset, see Backups in README.md}"

KEEP_HOURS=${BACKUP_KEEP_HOURS:-48}
KEEP_DAYS=${BACKUP_KEEP_DAYS:-7}
KEEP_WEEKS=${BACKUP_KEEP_WEEKS:-4}
KEEP_MONTHS=${BACKUP_KEEP_MONTHS:-6}

prune() {
  ls -t "$1"/*.sql.gz.age 2>/dev/null | tail -n +$(($2 + 1)) | xargs -r rm --
}

# The first dump of a period becomes that period's keeper.
promote() {
  local src=$1 dir=$2 key=$3
  mkdir -p "$dir"
  [ -e "$dir/$key.sql.gz.age" ] || ln "$src" "$dir/$key.sql.gz.age"
}

echo "[backup] hourly dumps -> /backups/hourly, encrypted to $BACKUP_AGE_RECIPIENT"

while true; do
  # Wait out a Postgres restart instead of losing the hour.
  until pg_isready -h "${POSTGRES_HOST:-postgres}" -U "$POSTGRES_USER" -q; do sleep 5; done

  if FILE=$(/usr/local/bin/dump.sh /backups/hourly lwt "$KEEP_HOURS"); then
    promote "$FILE" /backups/daily   "$(date '+%Y%m%d')"
    promote "$FILE" /backups/weekly  "$(date '+%GW%V')"
    promote "$FILE" /backups/monthly "$(date '+%Y%m')"
    prune /backups/daily "$KEEP_DAYS"
    prune /backups/weekly "$KEEP_WEEKS"
    prune /backups/monthly "$KEEP_MONTHS"
    echo "[backup] $(date '+%Y-%m-%d %H:%M:%S') $FILE"
  else
    # A failed backup is the moment to try more often, not less.
    echo "[backup] $(date '+%Y-%m-%d %H:%M:%S') FAILED, retrying in 5m" >&2
    sleep 300
    continue
  fi
  sleep $((3600 - $(date +%s) % 3600))
done

#!/bin/bash
# The pre-deploy backup. It runs the same dump.sh as the hourly sidecar, so these
# files are encrypted and restored the same way.
set -euo pipefail

KEEP=20
FILE=$(docker compose exec -T postgres-backup \
  /usr/local/bin/dump.sh /backups/deployments lwt-deploy "$KEEP" </dev/null | tr -d '\r')
echo "Backup complete -> ${FILE#/backups/} (encrypted; restore with ./restore.sh)"

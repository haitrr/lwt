#!/bin/bash
# The pre-deploy backup: the same encrypted dump as the hourly one, kept in
# deployments/ (last 20).
set -euo pipefail

FILE=$(docker compose exec -T postgres-backup pg-backup now </dev/null | tr -d '\r')
echo "Backup complete -> ${FILE#/backups/} (encrypted; restore with ./restore.sh)"

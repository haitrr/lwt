#!/bin/bash
# The pre-deploy backup: the same encrypted dump as the hourly one, kept in
# deployments/ (last 20).
set -euo pipefail

# The production stack, unless COMPOSE_FILE says otherwise (docker-compose.yaml builds from this checkout).
export COMPOSE_FILE=${COMPOSE_FILE:-docker-compose.prod.yml}

FILE=$(docker compose exec -T postgres-backup pg-backup now </dev/null | tr -d '\r')
echo "Backup complete -> ${FILE#/backups/} (encrypted; restore with ./restore.sh)"

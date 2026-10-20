#!/bin/bash
# Back up the database, then pull the latest images and restart the stack.
set -euo pipefail

# The production stack, unless COMPOSE_FILE says otherwise (docker-compose.yaml builds from this checkout).
export COMPOSE_FILE=${COMPOSE_FILE:-docker-compose.prod.yml}

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BOLD='\033[1m'
RESET='\033[0m'

STEPS=3
CURRENT=0

step() {
  CURRENT=$((CURRENT + 1))
  echo ""
  echo -e "${BLUE}${BOLD}[$CURRENT/$STEPS]${RESET} ${BOLD}$1${RESET}"
}
ok() { echo -e "  ${GREEN}✓${RESET} $1"; }
warn() { echo -e "  ${YELLOW}!${RESET} $1"; }
fail() { echo -e "  ${RED}✗ Error: $1${RESET}"; exit 1; }

cd "$(dirname "$0")"

echo -e "${BOLD}=== Deploying LWT ===${RESET}"
echo "Started at $(date '+%Y-%m-%d %H:%M:%S')"

step "Backing up database"
# The backup service isn't running on a first deploy, so there is nothing to back up yet.
if ./backup.sh; then ok "Backup complete"; else warn "Skipping backup, first deploy?"; fi

step "Pulling images"
# `up` alone keeps whichever copy of an image it pulled first, so without this
# neither a new app build nor the backup image's weekly rebuild would reach the host.
docker compose pull -q && ok "Images pulled" || warn "Could not pull every image, keeping the current ones"

step "Deploying app"
docker compose up -d && ok "App deployed" || fail "Docker deploy failed"

echo ""
echo -e "${GREEN}${BOLD}=== All done at $(date '+%Y-%m-%d %H:%M:%S') ===${RESET}"

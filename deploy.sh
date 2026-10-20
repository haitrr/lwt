#!/bin/bash
# Back up the database, then rebuild and restart the stack.
set -euo pipefail

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BOLD='\033[1m'
RESET='\033[0m'

STEPS=2
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

step "Building and deploying app"
docker compose up -d --build && ok "App deployed" || fail "Docker deploy failed"

echo ""
echo -e "${GREEN}${BOLD}=== All done at $(date '+%Y-%m-%d %H:%M:%S') ===${RESET}"

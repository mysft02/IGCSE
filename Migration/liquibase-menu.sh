#!/bin/bash

set -e

# Colors
BOLD='\033[1m'
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

LB_CMD="liquibase"
DEFAULTS_FILE="liquibase.properties"
CLASSPATH_JAR="mysql-connector-j-9.4.0.jar"

if [ ! -f "$DEFAULTS_FILE" ]; then
  echo -e "${RED}liquibase.properties not found in $(pwd)${NC}"
  exit 1
fi

if [ ! -f "$CLASSPATH_JAR" ]; then
  echo -e "${YELLOW}$CLASSPATH_JAR not found; Liquibase may fail to connect to MySQL.${NC}"
fi

if ! command -v "$LB_CMD" >/dev/null 2>&1; then
  echo -e "${YELLOW}Liquibase CLI not found in PATH.${NC}"
  echo -e "Using Docker fallback (liquibase/liquibase)."
  LB_CMD="docker run --rm -it -v \"$PWD\":/liquibase/changelog -w /liquibase/changelog liquibase/liquibase:latest"
fi

run_lb() {
  eval $LB_CMD --defaultsFile=$DEFAULTS_FILE --classpath=$CLASSPATH_JAR "$@"
}

pause() { read -p $'Press Enter to continue...'; }

while true; do
  clear
  echo -e "${BOLD}Liquibase Migration Menu${NC} - $(date '+%Y-%m-%d %H:%M:%S')"
  echo "----------------------------------------"
  echo "1) Update (apply changes)"
  echo "2) UpdateSQL (preview SQL)"
  echo "3) Status"
  echo "4) Validate"
  echo "5) Rollback last N changesets"
  echo "6) Tag database"
  echo "7) Rollback to tag"
  echo "8) History"
  echo "9) Generate Changelog (from existing DB)"
  echo "0) Exit"
  echo "----------------------------------------"
  read -rp "Select an option: " opt

  case "$opt" in
    1) run_lb update || true; pause ;;
    2) run_lb updateSQL || true; pause ;;
    3) run_lb status || true; pause ;;
    4) run_lb validate || true; pause ;;
    5) read -rp "Enter number of changesets to rollback: " n; [[ -n "$n" ]] && run_lb rollbackCount "$n" || true; pause ;;
    6) read -rp "Enter tag name: " tag; [[ -n "$tag" ]] && run_lb tag "$tag" || true; pause ;;
    7) read -rp "Enter tag name to rollback to: " tag; [[ -n "$tag" ]] && run_lb rollback "$tag" || true; pause ;;
    8) run_lb history || true; pause ;;
    9) read -rp "Output changelog file name (e.g., snapshot.yml): " out; [[ -n "$out" ]] && run_lb generate-changelog --format=yaml --changelog-file="$out" || true; pause ;;
    0) echo "Bye!"; exit 0 ;;
    *) echo -e "${RED}Invalid option${NC}"; sleep 1 ;;
  esac

done

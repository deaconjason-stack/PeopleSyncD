#!/usr/bin/env bash
set -euo pipefail

: "${PGHOST:?PGHOST is required}"
: "${PGPORT:=5432}"
: "${PGUSER:?PGUSER is required}"
: "${PGDATABASE:?PGDATABASE is required}"
: "${PGPASSWORD_FILE:?PGPASSWORD_FILE is required}"

if [[ ! -r "$PGPASSWORD_FILE" ]]; then
  echo "Password file is not readable" >&2
  exit 1
fi

export PGPASSWORD
PGPASSWORD="$(<"$PGPASSWORD_FILE")"
output_dir="${BACKUP_OUTPUT_DIR:-backups}"
mkdir -p "$output_dir"
timestamp="$(date -u +%Y%m%dT%H%M%SZ)"
output="$output_dir/${PGDATABASE}-${timestamp}.dump"

pg_dump --format=custom --no-owner --no-privileges --file="$output"
sha256sum "$output" > "$output.sha256"
echo "Backup created: $output"

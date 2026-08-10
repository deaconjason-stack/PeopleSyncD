#!/usr/bin/env bash
set -euo pipefail

backup_file="${1:-}"
if [[ -z "$backup_file" || ! -r "$backup_file" ]]; then
  echo "Usage: $0 <backup.dump>" >&2
  exit 2
fi

pg_restore --list "$backup_file" >/dev/null
if [[ -f "$backup_file.sha256" ]]; then
  sha256sum --check "$backup_file.sha256"
fi

echo "Backup structure and available checksum verified."
echo "A complete recovery exercise must also restore into an isolated database and run validation queries."

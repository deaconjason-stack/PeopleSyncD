#!/bin/sh
set -eu

: "${PGHOST:?PGHOST is required}"
: "${PGPORT:?PGPORT is required}"
: "${PGDATABASE:?PGDATABASE is required}"
: "${PGUSER:?PGUSER is required}"
: "${PGPASSWORD:?PGPASSWORD is required}"

MIGRATION_DIR="${MIGRATION_DIR:-/migrations}"

psql -v ON_ERROR_STOP=1 <<'SQL'
CREATE TABLE IF NOT EXISTS peoplesyncd_schema_migrations (
    migration text PRIMARY KEY,
    checksum text NOT NULL,
    applied_at timestamptz NOT NULL DEFAULT now()
);
SQL

for migration in "$MIGRATION_DIR"/*.sql; do
    [ -f "$migration" ] || continue

    name=$(basename "$migration")
    checksum=$(sha256sum "$migration" | awk '{print $1}')
    existing=$(psql -v ON_ERROR_STOP=1 -Atc \
        "SELECT checksum FROM peoplesyncd_schema_migrations WHERE migration = '$name';")

    if [ -n "$existing" ]; then
        if [ "$existing" != "$checksum" ]; then
            echo "Migration checksum mismatch: $name" >&2
            exit 1
        fi

        echo "Migration already applied: $name"
        continue
    fi

    echo "Applying migration: $name"
    psql -v ON_ERROR_STOP=1 -f "$migration"
    psql -v ON_ERROR_STOP=1 -c \
        "INSERT INTO peoplesyncd_schema_migrations (migration, checksum) VALUES ('$name', '$checksum');"
done

echo "PeopleSyncD database migration contract is current."

# Phase 7 PostgreSQL Runtime Quick Start

## Prerequisites

- Node.js 22 or newer
- npm 10 or newer
- PostgreSQL 16
- A database account authorized to apply migrations

## Apply migrations

Apply the SQL files in lexical order:

```bash
for migration in database/migrations/*.sql; do
  psql "$PEOPLESYNCD_DATABASE_URL" -v ON_ERROR_STOP=1 -f "$migration"
done
```

Migration `0002_phase7_postgres_runtime.sql` creates the constrained `peoplesyncd_runtime` role, grants the current migration account permission to assume it, and forces row-level security.

## Start the API with PostgreSQL

```bash
export NODE_ENV=development
export PEOPLESYNCD_STORAGE=postgres
export PEOPLESYNCD_DATABASE_URL='postgresql://USER:PASSWORD@HOST:5432/DATABASE'
export PEOPLESYNCD_SESSION_SECRET='replace-with-at-least-32-random-characters'
npm install
npm run dev:api
```

On Windows PowerShell:

```powershell
$env:NODE_ENV = 'development'
$env:PEOPLESYNCD_STORAGE = 'postgres'
$env:PEOPLESYNCD_DATABASE_URL = 'postgresql://USER:PASSWORD@HOST:5432/DATABASE'
$env:PEOPLESYNCD_SESSION_SECRET = 'replace-with-at-least-32-random-characters'
npm install
npm run dev:api
```

## Production safeguards

When `NODE_ENV=production`:

- memory storage is rejected;
- a database URL is required;
- an explicitly supplied session secret is required; and
- development-session issuance is disabled.

Do not deploy with a database owner or superuser credential. Phase 7A still uses a migration account that is permitted to assume the runtime role; separate migration and application credentials remain a production-certification requirement.

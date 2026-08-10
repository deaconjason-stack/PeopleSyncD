# Phase 7B2B1 Recovery and Administrative Safety Quick Start

## 1. Apply migrations with the migration identity

```bash
for migration in database/migrations/*.sql; do
  psql "$PEOPLESYNCD_MIGRATION_DATABASE_URL" -v ON_ERROR_STOP=1 -f "$migration"
done
```

The migration identity is used only for controlled schema changes. Do not provide it to the running API.

## 2. Provision a dedicated runtime login

Run the provisioning script as a database administrator. Supply values through protected process or secret-manager integration rather than committing them.

```bash
psql "$PEOPLESYNCD_MIGRATION_DATABASE_URL" \
  -v runtime_login=peoplesyncd_app \
  -v runtime_password="$PEOPLESYNCD_RUNTIME_PASSWORD" \
  -f scripts/database/provision_runtime_login.sql
```

The created LOGIN inherits the NOLOGIN `peoplesyncd_runtime` capability role.

## 3. Configure the API

Use the example in `config/environments/phase7b2b-recovery-safety.env.example`.

Production requires:

```text
NODE_ENV=production
PEOPLESYNCD_STORAGE=postgres
PEOPLESYNCD_DATABASE_ROLE_MODE=direct
PEOPLESYNCD_RUNTIME_DATABASE_URL=postgresql://<runtime-login>:<secret>@<host>/<database>
PEOPLESYNCD_DEV_AUTH=false
```

Session and MFA encryption keys must be different, randomly generated, and stored in the approved secret manager.

## 4. Verify the development build

```bash
npm install --no-audit --no-fund
npm run typecheck
npm test
npm run build
```

PostgreSQL integration tests require `PEOPLESYNCD_TEST_DATABASE_URL` and all migrations applied.

## 5. Recovery-code behavior

After initial TOTP verification, save the displayed recovery codes in a secure offline location. The platform stores only keyed hashes.

`POST /v1/auth/mfa/recovery/consume` accepts one code in the current authenticated session. Successful use rotates the session and returns the number of unused codes remaining. A used code is permanently rejected.

This route is not an unauthenticated account-recovery flow.

## 6. Founder safety

A request that would suspend, end, or remove membership-management authority from the final active Founder returns a conflict. Add and verify another authorized Founder before performing such a change.

## 7. Release boundary

The 0.4.1 package is unsigned and for controlled internal verification only. It is not certified for customer production deployment.

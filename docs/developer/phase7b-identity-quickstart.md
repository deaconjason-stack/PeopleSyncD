# Phase 7B1 Identity Beta Quick Start

## Apply migrations

Apply every SQL migration in lexical order using an authorized migration credential:

```bash
for migration in database/migrations/*.sql; do
  psql "$PEOPLESYNCD_MIGRATION_DATABASE_URL" -v ON_ERROR_STOP=1 -f "$migration"
done
```

## Start the API

```bash
export NODE_ENV=development
export PEOPLESYNCD_STORAGE=postgres
export PEOPLESYNCD_DATABASE_URL='postgresql://RUNTIME_USER:PASSWORD@HOST:5432/DATABASE'
export PEOPLESYNCD_SESSION_SECRET='replace-with-at-least-32-random-characters'
npm install
npm run dev:api
```

For the current engineering environment, the connected database user must be permitted to assume `peoplesyncd_runtime`. Production certification requires a dedicated deployed login and separate migration credential.

## Development session flow

The development-only endpoint creates a persistent session based on the governed Founder membership. The returned signed token contains the session identifier, but every later protected request also checks the server-side session and current membership.

Development session issuance is unavailable when `NODE_ENV=production`.

## MFA boundary

`POST /v1/auth/mfa/methods` creates a pending enrollment record only. No authenticator secret or recovery material is generated or returned. Do not present this endpoint as completed MFA.

## Federation boundary

The migration creates the external-identity mapping schema, but no OIDC or SAML callback is active. Production identity-provider configuration remains future work.

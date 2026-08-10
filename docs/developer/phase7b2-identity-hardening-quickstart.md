# Phase 7B2A Identity-Hardening Quick Start

## Purpose

Run the PeopleSyncD 0.4.0 identity-hardening build locally for engineering verification. This procedure is not a production deployment guide.

## Prerequisites

- Node.js 22
- npm
- PostgreSQL 16
- a database owner or migration credential
- a separate random session secret
- a separate random MFA encryption key

## Required environment

```text
NODE_ENV=development
PEOPLESYNCD_STORAGE=postgres
PEOPLESYNCD_DATABASE_URL=postgresql://<migration-user>:<password>@127.0.0.1:5432/peoplesyncd
PEOPLESYNCD_SESSION_SECRET=<at-least-32-random-characters>
PEOPLESYNCD_MFA_ENCRYPTION_KEY=<different-at-least-32-random-characters>
PEOPLESYNCD_DEV_AUTH=true
PEOPLESYNCD_CORS_ORIGIN=http://localhost:5173
```

Never commit real credentials. Production configuration requires an independently managed MFA encryption key and disables development authentication.

## Apply migrations

Run every SQL file in `database/migrations` in lexical order with stop-on-error behavior. The migration credential must be authorized to create or alter the constrained runtime role and schema objects.

## Install, verify, and build

```text
npm install --no-audit --no-fund
npm run typecheck
npm test
npm run build
```

For live PostgreSQL integration tests, set `PEOPLESYNCD_TEST_DATABASE_URL` to a disposable migrated database.

## Start services

```text
npm run dev -w @peoplesyncd/api
npm run dev -w @peoplesyncd/web
```

Default engineering endpoints:

- API health: `http://127.0.0.1:8080/health/ready`
- Web shell: `http://localhost:5173`

## Engineering identity flow

1. Request the development-only session endpoint.
2. Preserve the returned organization identifier and bearer token.
3. Start a TOTP enrollment through `POST /v1/auth/mfa/methods`.
4. Scan the returned provisioning URI with an authenticator application.
5. Verify the current code through `POST /v1/auth/mfa/totp/{methodId}/verify`.
6. Securely record the returned recovery codes; they are not returned again.
7. Replace the old bearer token with the rotated token.
8. Confirm the old token is denied and the replacement token is accepted.

## Current boundaries

- WebAuthn enrollment records exist, but registration and assertion ceremonies do not.
- Recovery codes are generated and hashed, but consumption is not implemented.
- OIDC, SAML, OAuth2, and SCIM are not implemented.
- The application still requires separate deployed-runtime database credentials before certification.
- The Windows installer is unsigned.
- This build is not certified for customer production use.

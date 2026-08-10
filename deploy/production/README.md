# PeopleSyncD Persistent Hosting Runbook

This package turns the verified GitHub source into immutable container images and a repeatable HTTPS deployment contract. It does not select or create a cloud host by itself.

## Architecture

- GitHub Actions builds the .NET API and Next.js web images.
- Images are pushed to GitHub Container Registry by digest.
- GitHub artifact attestations bind each published image digest to its build workflow.
- A managed PostgreSQL service remains outside the application host.
- A migration-only PostgreSQL identity applies the ordered `database/dotnet/*.sql` contract.
- A constrained runtime database identity is used by the API.
- Caddy is the only public container and terminates HTTPS on ports 80/443.
- `/api/*`, `/health`, `/alive`, and `/openapi/*` route to the API; all other requests route to Next.js on the same origin.

## Required infrastructure

1. A persistent Linux host with Docker Engine and Docker Compose v2.
2. A public DNS name pointing to that host.
3. A managed PostgreSQL database reachable from that host using TLS.
4. Separate migration and runtime database credentials.
5. Protected secret storage for the JWT signing key and database credentials.
6. Digest-pinned Caddy and PostgreSQL client images.
7. Access to the GHCR application packages when they are not public.

Only ports 80 and 443 should be internet-accessible on the application host.

## Publish application images

The `Publish GHCR Containers` workflow publishes:

- `ghcr.io/deaconjason-stack/peoplesyncd-api`
- `ghcr.io/deaconjason-stack/peoplesyncd-web`

Use the workflow evidence artifact to copy the exact `sha256:` digests into the deployment environment. Do not deploy the mutable `edge` tag.

## Configure the host

Copy `deploy/production/environment.example` to a secret-managed environment file outside version control and replace every placeholder.

The runtime `PEOPLESYNCD_DATABASE_CONNECTION_STRING` must use the constrained application user. The `PEOPLESYNCD_DB_MIGRATION_*` variables must use a separate migration identity.

The public domain is also the WebAuthn relying-party ID, and `https://<domain>` is the accepted browser origin.

## Deploy

From a checkout of the exact deployment commit:

```bash
docker compose --env-file /secure/path/peoplesyncd.env -f docker-compose.production.yml pull
docker compose --env-file /secure/path/peoplesyncd.env -f docker-compose.production.yml up --detach
```

The `migrate` service runs first. It creates a migration ledger, applies each ordered migration once, records its SHA-256 checksum, and refuses to continue if an already-applied migration file has changed.

After the migration service completes successfully, the API, web app, and HTTPS gateway start.

## Verify

Verify the public site and API from outside the host:

```bash
curl --fail --show-error https://YOUR_DOMAIN/health
curl --fail --show-error https://YOUR_DOMAIN/
```

Then verify registration, login, tenant selection, MFA, session management, and passkey ceremonies using non-production test accounts before enabling real users.

## Rollback

Application rollback is performed by restoring the previously approved API and web image digest references and redeploying the compose contract.

Database migrations are forward-only. Do not edit or delete an already-applied migration. A database recovery or rollback must follow a separately reviewed forward migration or a tested backup/restore procedure.

## Security boundary

This deployment contract is an engineering staging/production-hosting foundation. It does not by itself constitute production certification. Before real employee/customer data is accepted, PeopleSyncD still requires protected production secret custody, managed backup/restore evidence, monitoring/alerting, live-authenticator acceptance, hardened browser refresh-token custody, independent security review, and final deployment authorization.

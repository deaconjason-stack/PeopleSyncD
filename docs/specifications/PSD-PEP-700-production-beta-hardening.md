# PSD-PEP-700 — Production Beta Hardening

- **Status:** Active implementation
- **Phase:** 7
- **Release line:** 0.3.x production-beta foundation
- **Product:** PeopleSyncD Genesis
- **Owner:** MediSyncD Technologies

## 1. Purpose

Phase 7 converts the executable internal alpha into an evidence-driven production beta. It does not authorize public production use or claim certification.

The first controlled increment, Phase 7A, replaces volatile runtime records with a PostgreSQL repository protected by a constrained database role, transaction-scoped organization context, forced row-level security, append-only audit storage, and live integration tests.

## 2. Phase 7A implemented scope

The following capabilities are implemented and verified in CI:

1. Asynchronous `PlatformStore` and `AuditRepository` contracts.
2. PostgreSQL-backed Person, Worker, Audit, and Founder Dashboard data access.
3. In-memory adapter retained only for isolated tests and local prototype use.
4. Production startup fails closed unless PostgreSQL storage is configured.
5. Production startup requires an explicitly supplied session secret of at least 32 characters.
6. Development authentication remains unavailable in production mode.
7. Each PostgreSQL operation starts a transaction, assumes the constrained `peoplesyncd_runtime` role, and sets `app.organization_id` locally.
8. Row-level security is forced on Person, Worker, and Audit tables.
9. The runtime role receives only the minimum current table privileges.
10. Audit records remain append-only.
11. CI applies all migrations before tests and verifies forced row-level security.
12. Live PostgreSQL tests prove persistence and absence of cross-organization record leakage.
13. API and Windows build artifacts remain commit-linked, hashed, unsigned, and uncertified.

## 3. Data-access rules

All tenant-owned queries shall execute through the repository contract. Route handlers shall not issue direct SQL.

Every tenant transaction shall:

1. begin a database transaction;
2. assume `peoplesyncd_runtime`;
3. set `app.organization_id` using transaction-local configuration;
4. execute parameterized SQL;
5. commit on success or roll back on failure; and
6. release the connection.

Application-side organization checks remain mandatory. Database row-level security is defense in depth and shall not replace authorization middleware.

## 4. Runtime storage policy

- `memory` is permitted only for development, unit tests, and controlled demonstrations.
- `postgres` is required whenever `NODE_ENV=production`.
- A PostgreSQL URL must be supplied through `PEOPLESYNCD_DATABASE_URL` or an approved secret injection mechanism.
- Database owner or superuser credentials shall not be used by the deployed API.
- Migration credentials and runtime credentials shall be separated before production certification.

## 5. Health behavior

`/health/ready` reports the active storage adapter and returns HTTP 503 when the selected repository cannot reach its persistence dependency.

The health endpoint does not disclose credentials, database names, hostnames, or tenant information.

## 6. Verification

Phase 7A verification includes:

- migration execution on PostgreSQL 16;
- runtime-role creation and grants;
- forced row-level-security assertion;
- strict TypeScript compilation;
- unit and API contract tests;
- live repository persistence tests;
- live cross-tenant leakage tests;
- web and desktop compilation;
- SBOM and checksum generation; and
- Windows NSIS packaging.

## 7. Explicitly incomplete Phase 7 controls

The following controls remain required before production-beta approval:

- federated identity provider integration;
- MFA enrollment, challenge, recovery, and reset governance;
- persistent sessions, revocation, rotation, and device records;
- production secret manager integration;
- distinct migration and runtime database credentials;
- full Organization and membership persistence;
- Documents, Workflow, Notification, Licensing, and approval runtime wiring;
- browser and desktop end-to-end suites;
- accessibility verification;
- performance, load, soak, and resilience testing;
- backup restoration evidence;
- disaster-recovery exercise evidence;
- dependency vulnerability remediation evidence; and
- independent security testing.

## 8. Release truthfulness

Phase 7A is a production-beta foundation. It is not a certified production release, and its Windows installer remains unsigned.

No healthcare, government, employment-law, privacy, accessibility, security, or regulatory authorization is implied by implementation or CI success.

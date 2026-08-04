# Code Index

This directory is the navigation entry point for executable PeopleSyncD source. It does not duplicate or relocate the canonical code trees.

## Canonical implementation paths

- `apps/api/` — platform API runtime
- `apps/web/` — browser application
- `apps/desktop/` — desktop application and Windows packaging
- `apps/mobile/` — mobile application foundation
- `services/` — business and platform service foundations
- `packages/` — shared libraries, SDK, UI, authentication, permissions, audit, AI, telemetry, and common types
- `database/` — migrations, schemas, seeds, worker data model, constraints, indexes, and views
- `infrastructure/` — Docker, Kubernetes, Helm, Terraform, and monitoring foundations
- `tests/` — cross-service security, performance, integration, acceptance, and release verification
- `tools/` and `scripts/` — governed engineering, validation, and certification utilities

## Code authority rules

- Code implements approved requirements, specifications, decisions, and contracts.
- Contract-generated code remains subordinate to the approved source contract.
- Every protected operation carries actor, tenant, permission, correlation, and audit context.
- Production behavior may not depend on development-only authentication, synthetic identities, placeholder secrets, or founder shortcuts.
- Database changes require controlled migrations and verification.
- Executable changes require strict typing, review, tests, traceability, and release evidence.

Use `docs/traceability/master-lifecycle.csv` to navigate from business objective to implementation and evidence.

# Architecture Index

This directory is the cross-domain architecture entry point for PeopleSyncD. It does not replace the controlled architecture corpus in `docs/architecture/` or the approved specifications in `docs/specifications/`.

## Canonical views

- Enterprise Master Blueprint: `PEOPLESYNCD-ENTERPRISE-MASTER-BLUEPRINT-V1.0.md`
- Machine-readable model: `master-blueprint.json`
- Root architecture summary: `ARCHITECTURE.md`
- Controlled architecture documents: `docs/architecture/`
- Architecture specifications: `docs/specifications/`
- Decisions: `docs/adr/`
- Proposals: `docs/rfc/`
- API contracts: `docs/api/`
- Event contracts: `docs/asyncapi/`
- Data and migrations: `database/`
- Deployment architecture: `infrastructure/`

## Required architecture views

1. Business and operating context
2. Product and application map
3. System context and trust boundaries
4. Container, service, package, and dependency views
5. Domain and data architecture
6. Identity, tenant, permission, audit, and AI-control views
7. Deployment and network profiles
8. Observability, failure, recovery, and support views
9. Traceability from requirements to evidence

Architecture diagrams are generated or maintained in `diagrams/`. Significant architectural changes require an ADR or RFC and an update to affected views and traceability.

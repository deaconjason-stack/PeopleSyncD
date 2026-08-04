# 05 — Data

**Domain ID:** PSD-DOM-DATA-005  
**Accountable function:** Data Engineering and Data Governance  
**Purpose:** Govern transactional data, schemas, analytics, reporting, knowledge, catalog, lineage, quality, retention, and recovery.

## Canonical sources

- `database/`
- `docs/schemas/`
- `docs/requirements/security/PSD-REQ-DATA-000001.yaml`
- `docs/asyncapi/`
- `docs/traceability/`
- `infrastructure/monitoring/`
- Approved analytics and reporting specifications

## Required artifacts

- Enterprise domain model and ERD
- Versioned schemas, migrations, constraints, indexes, and views
- Data classification, ownership, stewardship, retention, and deletion rules
- Tenant-isolation and row-level-security design
- Event, analytics, warehouse, and future lakehouse models
- Reporting definitions and metric lineage
- Data catalog, provenance, and quality controls
- Backup, restoration, recovery-point, and recovery-time evidence
- Sanitized examples and synthetic test-data standards

## Rules

- PostgreSQL is the authoritative transactional store for the current implementation.
- Tenant-owned records require explicit organization context and protected access.
- Protected tables use forced row-level security where applicable.
- Migrations are forward-controlled, reviewed, tested, and recoverable.
- Audit and security evidence is append-only.
- Reports and AI-grounding sources must expose lineage and freshness.
- Real customer, worker, credential, health, tax, or identity data is prohibited in repository fixtures and examples.

## Completion gate

A data change is implementation-ready only when classification, ownership, schema, migration, tenant policy, retention, quality tests, lineage, backup impact, rollback strategy, and consuming contracts are approved.

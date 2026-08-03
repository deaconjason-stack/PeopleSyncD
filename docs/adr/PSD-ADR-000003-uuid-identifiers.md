---
id: PSD-ADR-000003
title: Use UUID Identifiers for Governed Records
version: 1.0.0
status: Accepted
classification: Internal
owner: Enterprise Architecture Office
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-110
---

# Use UUID Identifiers for Governed Records

## Context

PeopleSyncD records are created across services, deployments, imports, offline-capable clients, and migration processes. Sequential identifiers can expose volume and complicate distributed creation.

## Decision

Governed entities use UUID identifiers. Human-friendly employee numbers, document numbers, and case numbers are separate tenant-scoped display values.

## Alternatives considered

- database sequences as global identity
- composite tenant and sequence keys
- random strings without a common format

## Consequences

Identifiers can be generated without central coordination and reveal less operational information. UUIDs do not provide authorization and must always be combined with verified tenant scope.

## Verification

Contracts, schemas, migrations, indexes, imports, logs, and tests use the approved UUID representation consistently.

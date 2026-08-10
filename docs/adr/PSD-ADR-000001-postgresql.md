---
id: PSD-ADR-000001
title: Use PostgreSQL as the Primary Transactional Database
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
  - PSD-PEP-130
---

# Use PostgreSQL as the Primary Transactional Database

## Context

PeopleSyncD requires relational integrity, transactions, indexing, JSON support, row-level security, mature backup tooling, broad deployment support, and strong ecosystem compatibility.

## Decision

PostgreSQL is the primary transactional database for Genesis platform services. Service ownership, schemas, migrations, and access boundaries remain explicit.

## Alternatives considered

- Microsoft SQL Server
- MySQL or MariaDB
- document databases as the primary store
- one database technology per service from the beginning

## Consequences

PostgreSQL provides a consistent foundation and strong tenant controls. Teams must manage migrations carefully and avoid creating a shared-database monolith. Specialized stores may be added through approved ADRs.

## Migration and rollback

Schema changes use versioned migrations, compatibility testing, backup verification, and rollback or forward-fix plans.

## Verification

Migration, constraint, row-level security, backup, restore, performance, and failure tests are required before production certification.

# Worker Domain Specification

## Purpose

Define the tenant-scoped relationship between a Person and an Organization without conflating identity, employment, governance, authentication, or compensation.

## Core entities

- Worker Assignment
- Position reference
- Department reference
- Supervisor relationship
- Status history
- Onboarding reference
- Offboarding reference

## Worker types

Employee, contractor, volunteer, intern, instructor, and advisor. Board authority is modeled through the Board service rather than ordinary worker classification.

## State model

`Planned → Onboarding → Active → Leave or Suspended → Ended → Archived`

Transitions require authorization, current-state validation, effective date, reason where applicable, optimistic concurrency, audit evidence, and versioned event publication.

## Invariants

- Every assignment belongs to exactly one organization tenant.
- A worker assignment references an existing authorized person.
- End date cannot precede start date.
- Historical assignments are preserved.
- Directory views exclude highly confidential fields.
- Cross-tenant identifiers are rejected before domain processing.

## Traceability

- Governing specification: PEP-2010 and planned PSD-PEP-3010
- Requirements: `REQ-WORKER-*` transitioning to `PSD-REQ-HR-WORKER-*`
- API: `services/worker/api.yaml`
- Events: `services/worker/asyncapi.yaml`
- Database: `database/worker/`
- Verification: `services/worker/testing.md`

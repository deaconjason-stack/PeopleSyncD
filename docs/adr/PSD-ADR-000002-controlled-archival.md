---
id: PSD-ADR-000002
title: Preserve History Through Lifecycle State and Controlled Archival
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
  - PSD-PEP-120
---

# Preserve History Through Lifecycle State and Controlled Archival

## Context

Workforce, governance, document, workflow, licensing, and audit records require history. Unqualified hard deletion can damage investigations, customer operations, legal holds, traceability, and release evidence.

## Decision

Governed records use explicit lifecycle states and controlled archival by default. Permanent deletion is a separate approved operation subject to retention, legal hold, tenant offboarding, privacy obligations, and audit.

## Alternatives considered

- immediate hard deletion
- universal Boolean soft-delete flags without domain lifecycle
- event-only reconstruction

## Consequences

History remains available and domain states stay meaningful. Queries must explicitly distinguish active, ended, archived, and deletion-eligible records. Archival is not a substitute for retention and deletion policy.

## Verification

Tests cover active-record filtering, history retrieval, restoration where allowed, legal hold, retention expiration, tenant isolation, and deletion authorization.

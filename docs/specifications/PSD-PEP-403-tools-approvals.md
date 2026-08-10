---
id: PSD-PEP-403
title: AI Tool Registry and Approval Framework Specification
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: AI Platform Team
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-230
  - PSD-PEP-240
  - PSD-PEP-260
  - PSD-PEP-400
---

# AI Tool Registry and Approval Framework Specification

Each AI tool declares identifier, version, owner, purpose, input schema, output schema, permission, tenant behavior, risk class, approval requirement, idempotency strategy, rate limit, timeout, audit events, and rollback behavior.

## Risk classes

- Read-only low risk
- Draft or recommendation
- Reversible operational action
- High-impact or regulated action
- Prohibited action

High-impact actions require a named approver, fresh authorization, preview, explicit confirmation, stale-state revalidation, execution evidence, and post-action result. Prohibited actions are never registered.

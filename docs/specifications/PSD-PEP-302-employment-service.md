---
id: PSD-PEP-302
title: Employment and Assignment Service Specification
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: HR Domain
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-300
  - PSD-PEP-3010
  - PSD-PEP-220
---

# Employment and Assignment Service Specification

## Purpose

Manage time-bounded workforce relationships, employment status, organizational assignments, supervisors, departments, positions, and lifecycle history.

## Lifecycle

`Applicant Reference → Pending Hire → Onboarding → Active → Leave or Suspended → Ended → Archived`

Each transition requires current-state validation, permission, effective date, reason where applicable, concurrency protection, audit evidence, and versioned event publication.

## Invariants

- A Worker references one Person and one Organization.
- Employment and assignments retain effective-date history.
- Ended records are preserved.
- Supervisor relationships cannot cross tenant boundaries.
- Employment status does not automatically change board authority.
- Directory responses exclude highly confidential fields.

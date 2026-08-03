---
id: PSD-ADR-000004
title: Evaluate CQRS Selectively Rather Than Platform-Wide
version: 0.1.0
status: Proposed
classification: Internal
owner: Enterprise Architecture Office
review_cycle: Until Resolved
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-110
---

# Evaluate CQRS Selectively Rather Than Platform-Wide

## Context

Some PeopleSyncD workloads may benefit from separate write models and read projections, especially audit, search, reporting, and complex dashboards. Applying CQRS everywhere would add substantial operational and consistency complexity.

## Proposed decision

Use conventional transactional service models by default. Adopt CQRS only for a bounded context with demonstrated scale, security, performance, or projection needs and an approved service ADR.

## Success measures

A CQRS adoption must show measurable benefit, defined consistency expectations, replay and recovery behavior, tenant isolation, observability, and operational ownership.

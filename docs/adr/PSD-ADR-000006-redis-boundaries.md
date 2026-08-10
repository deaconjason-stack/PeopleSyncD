---
id: PSD-ADR-000006
title: Restrict Redis to Approved Ephemeral Workloads
version: 0.1.0
status: Proposed
classification: Internal
owner: Enterprise Architecture Office
review_cycle: Until Resolved
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-120
  - PSD-PEP-130
---

# Restrict Redis to Approved Ephemeral Workloads

## Context

Redis may improve caching, rate limiting, short-lived coordination, and background work. It can also create data-loss, tenant-key collision, and sensitive-cache risks when treated as an ungoverned database.

## Proposed decision

Use Redis only for approved ephemeral workloads with tenant-aware keys, bounded retention, encryption, authentication, eviction planning, and source-of-truth recovery. PostgreSQL remains authoritative for governed transactional records.

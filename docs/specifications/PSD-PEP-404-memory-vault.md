---
id: PSD-PEP-404
title: Domonique 2.0 Memory Vault Specification
version: 1.0.0
status: Approved
classification: Highly Confidential
owner: AI Platform Team
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-120
  - PSD-PEP-130
  - PSD-PEP-400
---

# Domonique 2.0 Memory Vault Specification

## Memory classes

Session context, user preference, tenant-approved fact, workflow continuity, founder-authorized memory, and restricted memory.

## Controls

- Memory is tenant-isolated, purpose-limited, encrypted, versioned, and auditable.
- The user can review, correct, export, or delete memory subject to legal hold.
- Highly confidential HR cases, credentials, secrets, authentication factors, and unrestricted document contents are not memorized by default.
- Memory writes require source, confidence, classification, owner, retention, and permission.
- Retrieval rechecks current authorization; past access never grants future access.
- Tenant data is not used to train shared models by default.

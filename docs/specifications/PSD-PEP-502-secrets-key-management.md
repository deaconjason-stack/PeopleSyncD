---
id: PSD-PEP-502
title: Secrets and Key Management
version: 1.0.0
status: Approved
classification: Highly Confidential
owner: Security Office
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-120
  - PSD-PEP-1250
  - PSD-PEP-1260
  - PSD-PEP-500
---

# Secrets and Key Management

## Purpose

Protect credentials, signing keys, encryption keys, certificates, tokens, integration secrets, and recovery material throughout their lifecycle.

## Controls

- Secrets reside in an approved secret manager or protected deployment mechanism.
- Keys and data are administratively separated.
- Access is least-privileged, time-bounded where possible, and audited.
- Rotation, revocation, backup, escrow where approved, and destruction are documented.
- Production signing keys remain owner-controlled and are never committed or embedded in applications.
- Compromise triggers immediate revocation, incident response, impact analysis, and reissuance.
- Secret values are excluded from logs, traces, support bundles, crash reports, and AI prompts.

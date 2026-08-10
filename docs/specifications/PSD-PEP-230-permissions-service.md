---
id: PSD-PEP-230
title: Permissions Service Specification
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: Security Office
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-120
  - PSD-PEP-130
  - PSD-PEP-140
---

# Permissions Service Specification

## Purpose

Provide centralized, default-deny authorization for users, services, integrations, and Domonique 2.0 tools.

## Policy inputs

Identity, organization, role, permission, action, record classification, relationship, scope, temporary grant, session risk, device trust, and approval state.

## Decision model

A decision returns allow or deny, policy version, evaluated scope, reason code, obligations, and correlation identifier. Clients may hide unavailable functions but cannot replace server authorization.

## Requirements

- Least privilege and deny by default.
- Time-bound grants expire automatically.
- Board authority does not grant unrelated personnel access.
- Manager scope is limited to authorized relationships.
- AI tools use the requesting user's policy context.
- Policy changes are versioned and audited.

## Verification

Policy-unit, scope, relationship, privilege-escalation, stale-grant, tenant-boundary, and AI-tool tests are mandatory.

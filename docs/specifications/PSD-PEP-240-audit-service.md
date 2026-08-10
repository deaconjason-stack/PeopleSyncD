---
id: PSD-PEP-240
title: Audit Service Specification
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
---

# Audit Service Specification

## Purpose

Accept, protect, correlate, retain, and expose authorized append-only evidence for business, security, governance, AI, and operational actions.

## Evidence envelope

Event ID, organization ID, actor, session, action, target, result, reason, occurred time, correlation ID, causation ID, policy version, AI involvement, and minimized change references.

## Integrity

Ordinary APIs cannot update or delete evidence. Storage, export, retention, legal hold, time synchronization, and integrity verification are governed controls.

## Privacy

Audit records minimize sensitive content and never become an unrestricted duplicate of personnel files.

## Verification

Append-only, ordering, integrity, tenant isolation, retention, search authorization, export, high-volume ingestion, and recovery tests are mandatory.

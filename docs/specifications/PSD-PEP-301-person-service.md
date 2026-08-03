---
id: PSD-PEP-301
title: Person Service Specification
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
  - PSD-PEP-110
  - PSD-PEP-120
  - PSD-PEP-300
---

# Person Service Specification

## Purpose

Maintain a stable human identity reference for HR and governance domains without conflating authentication, employment, board authority, or customer membership.

## Owned capabilities

- Person master record
- Names and preferred display name
- Contact methods
- Identity-reference deduplication
- Historical name and contact changes
- Data-classification metadata
- Record merge review workflow

## Exclusions

The Person service does not own login credentials, sessions, worker status, board appointments, compensation, payroll, medical details, accommodations, or background-check outcomes.

## Security

Sensitive identity attributes are minimized, encrypted where required, and excluded from general directory APIs. Record merges require authorized review and immutable evidence.

---
id: PSD-PEP-600
title: First Executable Platform Build
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: Engineering Organization
approver: Jason Henderson
review_cycle: Per Increment
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-100
  - PSD-PEP-120
  - PSD-PEP-130
  - PSD-PEP-300
  - PSD-PEP-400
  - PSD-PEP-500
---

# First Executable Platform Build

## Purpose

Move PeopleSyncD from repository specifications into a runnable internal alpha without weakening tenant isolation, human approval, audit, or certification truthfulness.

## Deliverables

- npm TypeScript workspaces
- Node.js API
- React Founder Dashboard
- Electron Windows shell
- signed development-session tokens
- explicit organization context
- default-deny permissions
- Person and Worker endpoints
- append-only in-memory audit ingestion
- read-only Domonique 2.0 Founder Brief tool
- PostgreSQL runtime migration
- unit, API, integration, and tenant-isolation tests
- unsigned Windows installer workflow
- commit-linked web, API, SBOM, checksum, and installer artifacts

## Boundary

The internal alpha uses an in-memory application repository while PostgreSQL migrations are validated independently. Production persistence, federated identity, signed Windows distribution, penetration testing, and production certification remain Phase 7 and Phase 8 work.

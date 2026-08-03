---
id: PSD-PEP-300
title: PeopleSyncD HR Domain Specification
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
  - PSD-PEP-100
  - PSD-PEP-110
  - PSD-PEP-120
  - PSD-PEP-130
  - PSD-PEP-140
---

# PeopleSyncD HR Domain Specification

## Purpose

Define the authoritative Human Resources bounded context for PeopleSyncD Enterprise Platform.

## Scope

The HR domain owns Person coordination, Worker relationships, Employment records, Assignments, Departments, Positions, Managers, Onboarding, Credentials, Training, HR Cases, worker-facing document coordination, lifecycle history, and workforce dashboards.

It reuses shared Identity, Organization, Permissions, Documents, Workflow, Audit, Notification, Licensing, Search, Reporting, and Domonique 2.0 platform services.

## Core principles

- A Person is not the same record as a Worker or user account.
- Historical workforce relationships are preserved rather than overwritten.
- Every HR record is organization-scoped and authorization-controlled.
- Highly confidential information is separated from ordinary directory views.
- High-impact actions require explicit human authority and complete audit evidence.
- Domonique 2.0 may assist, summarize, and prepare drafts, but cannot independently hire, terminate, discipline, change compensation, decide accommodations, or alter governance authority.

## Bounded subdomains

- Person coordination
- Worker and employment lifecycle
- Organization assignment
- Department and position management
- Onboarding and offboarding
- Credential and training management
- HR cases and concerns
- Workforce document coordination
- Workforce dashboards and reporting

## Authoritative board integration

Board authority remains owned by the Board service. The active MediSyncD Technologies board is Jason Henderson, Domonique Danielle Henderson, and Marietta Jessup only. HR records may reference board appointments but cannot create or alter governance authority without the Board service and required approval.

## Evidence chain

`Requirement → HR Specification → ADR → API/Event Contract → Database Contract → Implementation → Automated Tests → Certification`

---
id: PSD-PEP-220
title: Organization Service Specification
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: Organization Domain
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-110
  - PSD-PEP-130
---

# Organization Service Specification

## Purpose

Own tenant identity, company hierarchy, memberships, locations, business units, departments, teams, cost centers, branding, and explicit active-organization context.

## Core entities

Organization, Division, Department, Team, Location, Cost Center, Business Unit, Membership, and Organization Setting.

## Invariants

- Every tenant has one stable organization identifier.
- Hierarchies cannot contain cycles.
- Membership is explicit, time-bounded, and auditable.
- A caller can activate only an organization with valid membership.
- Parent-child relationships do not imply unrestricted data sharing.

## Events

Organization created, hierarchy changed, membership granted, membership ended, and context switched.

## Verification

Hierarchy, membership, tenant-context, cycle prevention, archival, concurrency, and cross-tenant tests are mandatory.

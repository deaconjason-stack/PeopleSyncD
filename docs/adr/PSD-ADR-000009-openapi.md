---
id: PSD-ADR-000009
title: Define REST Interfaces with OpenAPI
version: 1.0.0
status: Accepted
classification: Internal
owner: Enterprise Architecture Office
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-GOV-001
---

# Define REST Interfaces with OpenAPI

## Context

PeopleSyncD requires reviewable, testable, versioned interfaces shared by services, clients, SDKs, documentation, partners, and certification evidence.

## Decision

REST interfaces are defined contract-first using OpenAPI 3.1 or a later approved compatible version. Contracts include authentication, summaries, parameters, schemas, responses, errors, and security-relevant behavior.

## Consequences

Implementations and SDKs conform to the contract. Breaking changes require compatibility review, migration guidance, deprecation, and release evidence.

## Verification

CI validates syntax and policy. Contract tests verify implementation behavior before release.

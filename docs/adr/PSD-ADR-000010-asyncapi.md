---
id: PSD-ADR-000010
title: Define Asynchronous Interfaces with AsyncAPI
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
  - PSD-PEP-130
---

# Define Asynchronous Interfaces with AsyncAPI

## Context

Domain events need stable names, schemas, publishers, subscribers, ordering expectations, retries, retention, dead-letter behavior, security, and examples.

## Decision

Asynchronous interfaces are defined contract-first using AsyncAPI. Events use versioned names and a common envelope containing event ID, type, schema version, occurrence time, organization ID, correlation ID, and minimized data.

## Consequences

Producers and consumers can evolve independently within compatibility rules. Event contracts do not authorize data access and must exclude unnecessary sensitive content.

## Verification

CI validates contracts. Producer and consumer tests verify schema, idempotency, tenant handling, retry, and failure behavior.

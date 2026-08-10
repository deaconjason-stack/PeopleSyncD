---
id: PSD-ADR-000007
title: Standardize Observability on OpenTelemetry
version: 1.0.0
status: Accepted
classification: Internal
owner: DevSecOps Office
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-120
  - PSD-PEP-130
---

# Standardize Observability on OpenTelemetry

## Context

PeopleSyncD needs correlated logs, metrics, and traces across clients, APIs, services, events, databases, and infrastructure without permanent dependence on one monitoring vendor.

## Decision

OpenTelemetry conventions are the standard instrumentation foundation. Telemetry includes request ID, correlation ID, tenant ID, user or service identity where appropriate, session reference, service, operation, result, and timing.

## Consequences

Instrumentation becomes portable and consistent. Sensitive workforce content, secrets, tokens, document bodies, and prompt content are excluded by default.

## Verification

Tests and operational validation confirm correlation propagation, redaction, sampling, failure visibility, and exporter outage behavior.

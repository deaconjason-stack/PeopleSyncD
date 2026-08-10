---
id: PSD-STD-002
title: Artifact Identifiers and Traceability Standard
version: 1.0.0
status: Approved
classification: Internal
owner: Documentation Office
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-GOV-001
  - PSD-STD-001
  - PSD-REQ-000001
---

# Artifact Identifiers and Traceability Standard

## Global identifier families

- `PSD-GOV-*` governance
- `PSD-STD-*` standards
- `PSD-PEP-*` specifications
- `PSD-ADR-*` architecture decisions
- `PSD-RFC-*` requests for change
- `PSD-REQ-*` requirements
- `PSD-API-*` REST contracts
- `PSD-EVT-*` event contracts
- `PSD-SEC-*` security controls
- `PSD-TEST-*` automated tests
- `PSD-OPS-*` operations
- `PSD-CERT-*` certification
- `PSD-ERR-*` error codes

Identifiers are immutable and never reused. Superseded and deprecated artifacts remain historical.

## Mandatory evidence chain

`Requirement → Specification → Architecture Decision → Contract → Implementation → Automated Tests → Certification`

Each link records stable identifiers or repository paths. Missing links are visible exceptions, not silently assumed completion.

## Commit references

Governed implementation commits identify requirements, specifications, ADRs when applicable, and tests. Documentation-only commits identify the governed artifacts changed.

## Release generation

Release tooling generates requirement coverage, decision index, contract versions, schema changes, test evidence, and certification references from the authoritative records.

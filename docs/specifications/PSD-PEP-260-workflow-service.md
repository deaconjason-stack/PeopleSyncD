---
id: PSD-PEP-260
title: Workflow Service Specification
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: Workflow Domain
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-110
  - PSD-PEP-120
---

# Workflow Service Specification

## Purpose

Provide reusable state machines, tasks, approvals, deadlines, reminders, exceptions, retries, compensation, and evidence for onboarding and enterprise processes.

## Invariants

- Transitions are explicit and validated against current state.
- Commands are idempotent where required.
- High-impact transitions require authorized human approval.
- Separation-of-duties rules prevent prohibited self-approval.
- Failures preserve the last valid committed state.

## Events

Workflow started, task assigned, task completed, approval requested, approval decided, transition committed, transition rejected, and workflow completed.

## Verification

State, concurrency, idempotency, approval, timeout, retry, compensation, tenant isolation, and audit tests are mandatory.

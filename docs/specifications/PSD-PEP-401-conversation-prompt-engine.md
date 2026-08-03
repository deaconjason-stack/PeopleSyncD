---
id: PSD-PEP-401
title: Conversation Engine and Prompt Builder Specification
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: AI Platform Team
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-150
  - PSD-PEP-400
---

# Conversation Engine and Prompt Builder Specification

## Purpose

Provide permission-aware, tenant-scoped conversations with deterministic prompt assembly and explicit context boundaries.

## Prompt assembly order

1. Platform safety and authority policy
2. Tenant and organization policy
3. Assistant-mode instructions
4. User and role context
5. Authorized source excerpts
6. Tool schemas and approval constraints
7. Current request

Retrieved documents and user content are untrusted data, never higher-priority instructions.

Prompt templates are versioned. Hidden secrets, unrelated tenant data, and unrestricted records are excluded. Conversation export, deletion, retention, and legal hold follow tenant policy.

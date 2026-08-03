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

## Conversation model

A conversation records tenant, authenticated user, active organization, assistant mode, purpose, permitted context, model route, safety state, tool activity, sources, approvals, and retention policy.

## Prompt assembly order

1. Platform safety and authority policy
2. Tenant and organization policy
3. Assistant-mode instructions
4. User and role context
5. Authorized source excerpts
6. Tool schemas and approval constraints
7. Current request

Retrieved documents and user content are treated as untrusted data, never higher-priority instructions.

## Requirements

- Prompt templates are versioned and reviewable.
- Hidden instructions, secrets, unrestricted records, and unrelated tenant data are excluded.
- Context-window pressure uses deterministic summarization and source preservation.
- Conversation deletion, export, and retention follow tenant policy and legal hold.
- The platform must continue core non-AI operations when the model provider is unavailable.

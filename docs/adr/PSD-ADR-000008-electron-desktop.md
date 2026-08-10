---
id: PSD-ADR-000008
title: Use Electron for the Initial Windows Desktop Shell
version: 0.1.0
status: Proposed
classification: Internal
owner: Desktop Platform Team
review_cycle: Until Resolved
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-100
  - PSD-PEP-120
---

# Use Electron for the Initial Windows Desktop Shell

## Context

PeopleSyncD needs a Windows desktop application sharing the governed web experience while supporting secure local integration, notifications, updates, and future voice access.

## Proposed decision

Use Electron for the first desktop shell, subject to a security prototype validating sandboxing, process isolation, updater signing, deep-link handling, secure storage, permissions, packaging size, and performance.

## Alternatives considered

- Tauri
- .NET native client
- progressive web application only
- separate native clients

## Approval condition

This decision remains proposed until the desktop threat model and prototype evidence are reviewed.

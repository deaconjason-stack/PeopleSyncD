---
id: PSD-STD-001
title: PeopleSyncD Documentation Standard
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
  - PSD-STD-100
  - PSD-STD-110
---

# PeopleSyncD Documentation Standard

## Scope

Applies to controlled Markdown, YAML, JSON Schema, OpenAPI, AsyncAPI, SQL, Mermaid, operational runbooks, test evidence, release artifacts, and certification records.

## Required metadata

Controlled narrative documents begin with YAML front matter containing:

- `id`
- `title`
- `version`
- `status`
- `classification`
- `owner`
- `approver` when approval exists
- `review_cycle`
- `created`
- `updated`
- `supersedes`
- `references`

## Document states

`Draft → Review → Approved → Implemented → Verified → Released → Archived`

Status changes require review evidence. Released documents are bound to a release manifest.

## Writing rules

Documents must:

- use precise, testable language
- distinguish requirements from guidance
- distinguish targets from verified facts
- link related identifiers
- state security, privacy, accessibility, compatibility, migration, and operational effects when applicable
- avoid embedding secrets or real workforce data
- preserve historical decisions through supersession rather than destructive rewriting

## Machine validation

Automation validates front matter, identifier uniqueness, required fields, cross-references, contract syntax, diagrams, links, and traceability.

## Definition of complete

Documentation is complete only when the governed behavior, contracts, implementation, tests, and release evidence agree.

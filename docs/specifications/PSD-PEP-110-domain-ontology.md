---
id: PSD-PEP-110
title: PeopleSyncD Domain Ontology
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: Enterprise Architecture Office
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-100
  - PSD-GOV-001
---

# PeopleSyncD Domain Ontology

## Purpose

Define the canonical language, ownership boundaries, identities, relationships, and lifecycle concepts used across the PeopleSyncD Enterprise Platform.

## Bounded contexts

PeopleSyncD is organized into independently governed bounded contexts:

- Identity
- Organizations
- Permissions
- Human Resources
- Governance and Board
- Documents
- Workflow
- Notifications
- Audit
- Search
- Reporting and Analytics
- Licensing
- Configuration
- Integrations
- Domonique 2.0 AI

Each context owns its domain rules, data, contracts, events, terminology, operations, and verification evidence. A context may reference another context through stable identifiers and approved contracts but must not directly redefine another context's invariants.

## Canonical entities

### Person

A human identity record independent from authentication, employment, board authority, or customer access. One Person may have multiple organization memberships and worker assignments.

### User

An authenticated digital identity permitted to access PeopleSyncD. A User may reference a Person but is not the Person record itself.

### Organization

The primary tenant and authorization boundary. An Organization may contain divisions, business units, departments, teams, locations, and cost centers.

### Worker Assignment

A time-bounded relationship between a Person and an Organization. It records worker type, status, position, department, supervisor, dates, and lifecycle references.

### Board Appointment

A time-bounded grant of governance authority. Board Appointment is separate from employment and ordinary management permissions.

### Role and Permission

A Role groups governed Permissions. Authorization decisions also consider tenant, record relationship, classification, action, risk, conditions, and temporary grants.

### Document

A governed metadata record containing one or more immutable versions, classification, access rules, retention, legal-hold, expiration, and signature information.

### Workflow Instance

A stateful business process with tasks, transitions, deadlines, approvals, exceptions, retries, and audit evidence.

### Audit Event

Append-only evidence of an attempted or completed action. Audit Events are not editable business records.

### AI Conversation

A permission-aware interaction between a User and Domonique 2.0. AI Conversations contain governed messages, source references, tool calls, approvals, and retention controls.

### License

A commercial entitlement defining customer, edition, validity, deployment rights, features, limits, and contract reference.

## Relationship rules

- Authentication does not imply organization membership.
- Organization membership does not imply worker status.
- Worker status does not imply board authority.
- Board authority does not imply unrestricted personnel access.
- A document reference does not imply permission to retrieve its content.
- Domonique 2.0 receives no authority beyond the requesting user and approved tool.
- Historical relationships are ended or archived rather than silently overwritten.

## Identifier rules

Domain records use globally unique stable identifiers. External display numbers may be human-friendly but are not primary identity. Identifiers are immutable and tenant context is validated independently.

## State and history

State changes require explicit transitions, authorization, effective time, reason where applicable, concurrency control, audit evidence, and versioned events. Historical facts remain queryable according to authorization and retention policy.

## Data classification

Domain fields are classified as Public, Internal, Confidential, or Highly Confidential. Highly Confidential fields include identity documents, tax data, credentials, compensation, health and accommodation data, screening reports, investigation records, and privileged governance material.

## Success criteria

The ontology succeeds when service specifications and contracts use consistent language, ownership is unambiguous, cross-context coupling is visible, and developers can identify the authoritative source for every domain rule.

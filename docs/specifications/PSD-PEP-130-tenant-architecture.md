---
id: PSD-PEP-130
title: PeopleSyncD Multi-Tenant Architecture
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
  - PSD-PEP-110
  - PSD-PEP-120
  - PSD-REQ-PLATFORM-000001
---

# PeopleSyncD Multi-Tenant Architecture

## Purpose

Define Organization-based isolation for shared, dedicated, private-cloud, on-premises, hybrid, and restricted deployments.

## Tenant model

An Organization is the primary tenant boundary. Parent-child organization relationships describe hierarchy but do not automatically grant data access between organizations.

Every tenant-owned record carries an immutable `organization_id`. Requests carry authenticated tenant context selected only from active memberships. User-provided tenant identifiers are never trusted without server validation.

## Isolation controls

### API and application

The gateway and each service validate organization context before loading or modifying records. Record lookups use both record ID and organization ID. Unauthorized records should normally appear not found rather than revealing existence.

### Database

Tenant-owned tables include organization identifiers, supporting indexes, foreign-key consistency, and row-level security or equivalent database enforcement. Application connection context is set transactionally and cleared safely.

### Object storage

Document keys are tenant-scoped. Delivery uses short-lived authorization after server-side permission evaluation. Buckets and prefixes must not become public by default.

### Search and analytics

Indexes, queries, exports, data marts, and warehouses preserve tenant boundaries. Cross-tenant analytics require explicit governed aggregation and de-identification rules.

### Events and messaging

Every domain event contains organization ID, event ID, schema version, correlation ID, and minimized data. Consumers verify tenant context and process idempotently. Dead-letter records remain access-controlled.

### Cache

Cache keys include tenant and authorization-relevant context. Sensitive responses are not cached without explicit policy. Shared caches must prevent key collision and cross-tenant inference.

### Domonique 2.0

AI tools receive explicit organization scope and retrieve only authorized records. Retrieved content is treated as untrusted data, not instructions. Conversation memory is tenant-scoped and permission-aware.

### Operations and support

Administrative support access is time-bounded, purpose-limited, approved, visible, and audited. Support tools must not provide unrestricted cross-tenant browsing.

## Deployment patterns

Supported target patterns include:

- shared application and shared database with enforced row isolation
- shared application with dedicated database
- dedicated private-cloud deployment
- customer-managed on-premises deployment
- hybrid integration with customer systems
- restricted environment without public network dependency

The selected pattern is recorded per customer license and deployment manifest.

## Tenant lifecycle

Provisioning, activation, configuration, suspension, export, retention, offboarding, deletion, backup, restoration, and legal hold are governed workflows. License expiration does not destroy customer data.

## Verification

Automated tests cover cross-tenant IDs, direct object references, nested relationships, file retrieval, search, cache, events, exports, AI tools, backups, restores, and administrative support access.

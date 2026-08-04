# 04 — Platform

**Domain ID:** PSD-DOM-PLAT-004  
**Accountable function:** Platform Architecture and Engineering  
**Purpose:** Provide reusable services, contracts, runtime foundations, infrastructure interfaces, and engineering guardrails for every PeopleSyncD application.

## Canonical sources

- `ARCHITECTURE.md`
- `docs/architecture/`
- `docs/specifications/PSD-PEP-200-system-architecture.md` when approved
- `docs/specifications/PSD-PEP-210-identity-service.md` through `PSD-PEP-280-licensing-service.md`
- `apps/`
- `services/`
- `packages/`
- `docs/api/`
- `docs/asyncapi/`
- `infrastructure/`

## Shared services

Identity, Organizations, Permissions, Licensing, Workflow, Notifications, Documents, Audit, Search, Reporting, Configuration, Integration Hub, Telemetry, Localization, Accessibility, and Domonique 2.0 platform capabilities.

## Required artifacts

- Service boundaries, ownership, SLOs, and dependency maps
- OpenAPI, AsyncAPI, and schema contracts before implementation
- Generated server and client compatibility rules
- Kubernetes, infrastructure, CI/CD, observability, performance, and capacity designs
- Versioning, deprecation, migration, failure, retry, idempotency, and rollback policies
- Reference implementations and reusable templates

## Rules

- Applications consume shared services through approved contracts.
- Tenant and actor context must propagate across synchronous and asynchronous boundaries.
- Services default deny when required context or authority is absent.
- Generated code is subordinate to its approved source contract.
- Service-specific implementations may not weaken platform security or audit guarantees.
- Every service must define health, telemetry, deployment, testing, backup, recovery, and support behavior.

## Completion gate

A service is implementation-ready only when its owner, domain model, requirements, architecture, contracts, security model, data model, telemetry, test strategy, deployment profile, and operational runbook are approved.

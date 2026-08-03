# PeopleSyncD Enterprise Platform

- **Internal codename:** Genesis
- **Version target:** 1.0 Enterprise Release
- **Category:** AI-powered enterprise workforce operating system
- **Repository role:** Single Authoritative Source
- **Classification:** Commercial Confidential

PeopleSyncD HR is the first business application. The reusable enterprise platform underneath it is the long-term product.

## Mandatory evidence chain

`Requirement → Specification → Architecture → Contract → Database → Security → Implementation → Automated Tests → Release → Certification`

No stage may silently redefine an earlier stage. An undocumented or untraceable capability is not an approved part of PeopleSyncD.

## Deployment targets

- Cloud SaaS
- Private cloud
- On-premises
- Hybrid enterprise
- Government restricted environments

These are engineering targets, not current certifications.

## Current board authority

The active MediSyncD Technologies board consists only of Jason Henderson, Domonique Danielle Henderson, and Marietta Jessup.

## Repository product factory

- `apps/` — web, desktop, and mobile clients
- `services/` — bounded-context platform and application services
- `packages/` — reusable UI, authentication, permissions, telemetry, shared, and SDK packages
- `infrastructure/` — Docker, Kubernetes, Terraform, Helm, and monitoring
- `database/` — migrations, schemas, seeds, and service-owned contracts
- `docs/` — architecture, specifications, APIs, operations, security, certification, and licensing
- `tests/` — unit, contract, integration, security, performance, and acceptance evidence
- `tools/` and `scripts/` — corpus and build automation

## Status

Genesis is in governance and architecture foundation status. Requirement counts, test counts, coverage, enterprise readiness, certification, and signed-build status are generated facts and must not be asserted without evidence.

# 13 — Deployment

**Domain ID:** PSD-DOM-DEPLOY-013  
**Accountable function:** Platform Engineering, Site Reliability, Security, and Customer Operations  
**Purpose:** Define, build, verify, and support deployment profiles for SaaS, private, customer-managed, government, air-gapped, hybrid, and edge environments.

## Canonical sources

- `infrastructure/`
- `config/`
- `docs/specifications/PSD-PEP-500-enterprise-deployment.md`
- `docs/specifications/PSD-PEP-501-environment-configuration.md`
- `docs/specifications/PSD-PEP-502-secrets-key-management.md`
- `docs/specifications/PSD-PEP-503-backup-disaster-recovery.md`
- `docs/specifications/PSD-PEP-504-observability-support.md`
- `docs/specifications/PSD-PEP-505-supply-chain-release-security.md`
- `docs/releases/`
- `release/`

## Deployment profiles

- Multi-tenant SaaS
- Private cloud
- Customer-managed on-premises
- Hybrid enterprise
- Government restricted environment
- Air-gapped environment
- Edge deployment

## Required artifacts

- Profile-specific architecture, prerequisites, sizing, networking, identity, and storage
- Environment configuration and secret-management procedures
- Installation, upgrade, migration, rollback, and uninstall guidance
- Observability, support diagnostics, incident, and maintenance procedures
- Backup, restoration, disaster-recovery, and continuity evidence
- Capacity, performance, availability, and resilience tests
- SBOM, checksums, signatures, provenance, and release manifests
- Customer acceptance, support boundary, and end-of-life documentation

## Rules

- A deployment target is not supported merely because a container, chart, or installer exists.
- Migration and runtime database identities are separated.
- Production secrets are independently managed outside source control.
- Every profile preserves tenant, identity, permission, audit, privacy, licensing, and AI-governance controls.
- Air-gapped and government profiles require explicit dependency, update, export, and support procedures.
- Customer deployment requires an explicit release decision approving that profile and intended use.

## Completion gate

A deployment profile is supported only after clean installation, upgrade, rollback, backup, restoration, monitoring, incident response, security, performance, documentation, and customer-acceptance procedures are verified for the exact release artifacts.

# 07 — Engineering

**Domain ID:** PSD-DOM-ENG-007  
**Accountable function:** Engineering and Quality  
**Purpose:** Define how PeopleSyncD code, contracts, tests, repositories, environments, reviews, and releases are designed and maintained.

## Canonical sources

- `CONTRIBUTING.md`
- `VERSIONING.md`
- `ARCHITECTURE.md`
- `templates/service/`
- `tools/`
- `scripts/`
- `tests/`
- `.github/workflows/`
- Application, service, package, database, and infrastructure source trees

## Required artifacts

- Coding, naming, repository, dependency, and review standards
- Local development and test-environment instructions
- Service and package templates
- Contract-generation and compatibility procedures
- Unit, integration, security, performance, accessibility, and acceptance test strategies
- Quality gates, defect policy, release branching, migration, and rollback procedures
- Developer handbook, onboarding, ownership, and escalation paths
- Reproducible build, SBOM, checksum, and signing processes

## Rules

- Documentation and approved contracts precede implementation.
- Strict type checking and automated tests are mandatory for executable changes.
- Every feature includes acceptance criteria and test evidence.
- Significant architectural change requires an ADR or RFC.
- Database changes use controlled migrations and live migration verification.
- Build artifacts are tied to exact commits and workflows.
- A green pipeline is required but does not independently establish production certification.
- Technical debt and temporary exceptions require owners and removal dates.

## Completion gate

A change is merge-ready only when scope, ownership, review, contracts, tests, security impact, documentation, migration and rollback behavior, traceability, and required CI gates are complete.

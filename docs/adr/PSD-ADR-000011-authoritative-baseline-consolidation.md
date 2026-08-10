---
id: PSD-ADR-000011
title: Consolidate the PR 1-9 Lineage as the Authoritative Baseline
version: 1.0.0
status: Accepted
classification: Internal
owner: Enterprise Architecture Office
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-10
updated: 2026-08-10
supersedes: null
references:
  - PSD-PLAN-IMPLEMENT-001
---

# Consolidate the PR 1-9 Lineage as the Authoritative Baseline

## Context

PeopleSyncD has two histories descended from repository commit `37330332c9d8ad82f7157fe8f7cc652a15fd0952`.

The first is the stacked PR #1 through PR #9 lineage ending at `fcca0420c06974e5585710f70fe2eb5776ea0f91`. It contains the governed enterprise corpus, Genesis implementation, .NET 9 solution, tenant identity lifecycle, MFA, passkeys, privileged reauthentication, GitHub Codespaces deployment, and persistent GHCR hosting contract. Its milestone and deployment workflows produced commit-linked evidence.

The second is the current `main` lineage ending remotely at `2fbedb9d2ecea7f1e8dd6ae29878f31cecc049ef`. It contains a smaller .NET 10 API, PostgreSQL Compose file, Next.js shell, and three foundation workflows. Many paths were independently added in both lineages, but their dependency graphs, application layouts, persistence models, and security boundaries are not directly compatible.

A conventional conflict-by-conflict merge would create a third, unverified architecture. Preferring the `main` versions of overlapping files would remove verified M2.1-M2.4 behavior. Importing all non-overlapping `main` files would also leave duplicate APIs, web shells, migrations, and workflows that appear authoritative without sharing the same contracts.

## Decision

The complete PR #1 through PR #9 lineage is the authoritative PeopleSyncD implementation baseline.

The current `main` lineage will be recorded as merged using Git's `ours` merge strategy. This preserves every `main` commit and its content in recoverable Git history while retaining the PR #9 tree as the resulting source tree. The divergent .NET 10/minimal Next.js scaffold is formally retired as a superseded experimental baseline and will not be copied into active application paths.

The consolidation pull request will remain a draft until its applicable GitHub Actions pass. Merging that pull request into `main` requires a separate founder approval. The consolidation does not certify a production release or authorize a customer deployment.

Runtime modernization from .NET 9 to .NET 10 remains a separate future increment. That increment must upgrade the authoritative M2.4 implementation in place with dependency, migration, security, compatibility, and rollback evidence.

## Alternatives considered

- Prefer every `main` conflict resolution and rebuild the missing M2.1-M2.4 behavior afterward. Rejected because it would knowingly replace verified security controls with a smaller scaffold.
- Merge both trees file by file. Rejected because duplicate APIs, web layouts, migrations, and workflows would not form one coherent or tested architecture.
- Leave all pull requests open and continue stacking new work. Rejected because it prolongs the absence of one authoritative integration baseline.
- Delete or rewrite the `main` lineage. Rejected because repository history and prior decisions must remain auditable and recoverable.

## Security, privacy, and compliance impact

The decision retains the implemented tenant isolation, live membership validation, session revocation, TOTP, recovery-code, WebAuthn, privileged freshness, audit, migration checksum, and same-origin deployment controls. It avoids presenting the smaller scaffold as equivalent to those controls.

No credentials, production data, signing keys, or regulated records are introduced by the consolidation. Existing statements that the platform is not production certified remain in force.

## Data and migration impact

The authoritative PostgreSQL migration contracts in `database/migrations` and `database/dotnet` remain unchanged. The independent initial-schema file from the retired scaffold is preserved only in Git history and is not added to the active migration chain.

No production database migration is authorized by this decision.

## Compatibility and rollback

The consolidation branch retains both histories as parents. The authoritative pre-consolidation tree remains available at `fcca0420c06974e5585710f70fe2eb5776ea0f91`, and the retired remote `main` scaffold remains available at `2fbedb9d2ecea7f1e8dd6ae29878f31cecc049ef`.

Before the pull request is merged, rollback consists of closing the draft PR. After an approved merge, rollback must use a normal revert or follow-up branch; shared history must not be rewritten.

## Verification

- Confirm both lineage heads are ancestors of the consolidation branch.
- Confirm the history-recording merge does not change the selected authoritative tree.
- Run corpus, blueprint, traceability, and secret-scanning checks.
- Run TypeScript typecheck, test, and build gates.
- Require GitHub Actions for .NET restore, formatting, build, tests, PostgreSQL migration assertions, OpenAPI, containers, Codespaces, persistent hosting, and Windows packaging.
- Require an explicit founder decision before merging the consolidation PR into `main`.

## Consequences

PeopleSyncD obtains one reviewable integration baseline without losing either branch history. The active source remains coherent with the previously verified M2.4 and deployment evidence.

The .NET 10 scaffold is not active after consolidation. Any useful ideas from it must be reintroduced as requirements against the authoritative architecture rather than copied without compatibility and security verification.

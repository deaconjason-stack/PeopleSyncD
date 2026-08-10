# PeopleSyncD Baseline Consolidation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Consolidate the complete PR #1-#9 branch stack and the current `main` history into one reviewable PeopleSyncD baseline without replacing the verified M2.4 platform with the divergent minimal scaffold.

**Architecture:** Use `origin/deployment/persistent-ghcr` (`fcca0420c06974e5585710f70fe2eb5776ea0f91`) as the authoritative implementation tree because it contains the complete stacked governance, identity, MFA, passkey, Codespaces, and persistent-hosting work. Record `main` (`78177d888298e970458b87b445bee3a393380f6f`) as merged with Git's `ours` strategy, preserving every main commit in history while formally retiring its incompatible .NET 10/minimal Next.js scaffold through an accepted ADR.

**Tech Stack:** Git, .NET 9, ASP.NET Core, PostgreSQL 16, Next.js 16, React 19, TypeScript, Electron, Docker Compose, GitHub Actions

## Global Constraints

- Preserve the complete ancestry of PRs #1 through #9 and current `main`.
- Keep the verified M2.4 security implementation and persistent-hosting contracts authoritative.
- Do not silently combine incompatible .NET 9 and .NET 10 dependency graphs or the two unrelated web application layouts.
- Do not claim production certification, signed distribution, or a 24/7 deployment.
- Do not merge the final pull request into `main` without separate founder approval.
- Run all locally available checks and require GitHub Actions for .NET, PostgreSQL, Docker, and Windows validation unavailable in the local sandbox.

---

### Task 1: Record the consolidation decision

**Files:**

- Create: `docs/adr/PSD-ADR-000011-authoritative-baseline-consolidation.md`
- Modify: `docs/adr/README.md`
- Create: `docs/superpowers/plans/2026-08-10-peoplesyncd-baseline-consolidation.md`

**Interfaces:**

- Consumes: PR #9 head `fcca0420c06974e5585710f70fe2eb5776ea0f91` and local `main` head `78177d888298e970458b87b445bee3a393380f6f`.
- Produces: accepted decision `PSD-ADR-000011` and an auditable consolidation plan.

- [x] **Step 1: Write ADR PSD-ADR-000011**

Document the two lineages, the authoritative-tree decision, preserved history, rejected alternatives, rollback path, and verification gates. State explicitly that the .NET 10 scaffold remains recoverable from Git history but is not copied into the authoritative tree.

- [x] **Step 2: Register the ADR**

Add `PSD-ADR-000011` to `docs/adr/README.md` with status `Accepted`.

- [x] **Step 3: Validate documentation and identifiers**

Run:

```bash
npx --yes markdownlint-cli2 docs/adr/README.md docs/adr/PSD-ADR-000011-authoritative-baseline-consolidation.md docs/superpowers/plans/2026-08-10-peoplesyncd-baseline-consolidation.md
python scripts/governance/validate_master_blueprint.py
```

Expected: both commands exit `0`.

- [x] **Step 4: Commit the decision**

```bash
git add docs/adr/README.md docs/adr/PSD-ADR-000011-authoritative-baseline-consolidation.md docs/superpowers/plans/2026-08-10-peoplesyncd-baseline-consolidation.md
git commit -m "docs: govern authoritative baseline consolidation"
```

### Task 2: Consolidate the branch histories

**Files:**

- Modify: `.gitignore`
- Merge metadata: `main` into `agent/consolidate-peoplesyncd-baseline`

**Interfaces:**

- Consumes: accepted `PSD-ADR-000011` and local `main` at `78177d888298e970458b87b445bee3a393380f6f`.
- Produces: a merge commit whose first parent is the complete PR #1-#9 lineage and whose second parent is current `main`.

- [x] **Step 1: Preserve local worktree isolation**

Add the following generated-workspace rules to `.gitignore`:

```gitignore
.worktrees/
*.tsbuildinfo
```

- [x] **Step 2: Commit the generated-workspace rules**

```bash
git add .gitignore docs/superpowers/plans/2026-08-10-peoplesyncd-baseline-consolidation.md
git commit -m "chore: preserve consolidated worktree isolation"
```

- [x] **Step 3: Capture the authoritative tree before merging**

```bash
git rev-parse HEAD^{tree} > /tmp/peoplesyncd-authoritative-tree.txt
```

- [x] **Step 4: Record current main ancestry without importing the superseded scaffold tree**

```bash
git merge --strategy=ours --no-edit main
```

Expected: one merge commit with no content conflicts.

- [x] **Step 5: Verify both histories and the selected tree**

```bash
git merge-base --is-ancestor fcca0420c06974e5585710f70fe2eb5776ea0f91 HEAD
git merge-base --is-ancestor 78177d888298e970458b87b445bee3a393380f6f HEAD
test "$(git rev-parse HEAD^{tree})" = "$(cat /tmp/peoplesyncd-authoritative-tree.txt)"
test "$(git rev-list --parents -n 1 HEAD | wc -w)" = "3"
```

Expected: every command exits `0`.

### Task 3: Verify the consolidated baseline

**Files:**

- Test only; no expected source changes.

**Interfaces:**

- Consumes: consolidated branch tree and installed dependencies.
- Produces: local verification evidence and GitHub Actions evidence after publication.

- [x] **Step 1: Verify repository state and merge ancestry**

```bash
git status --short --branch
git log --graph --decorate --oneline --max-count=20
git diff --check origin/deployment/persistent-ghcr..HEAD
```

Expected: no uncommitted files and no whitespace errors.

- [x] **Step 2: Verify the Genesis TypeScript platform**

```bash
npm install --no-audit --no-fund --package-lock=false
npm run typecheck
npm test
npm run build
```

Expected on a normal Linux host: all commands exit `0`. If the sandbox lacks `/proc`, record the `uv_resident_set_memory` limitation and use GitHub Actions as the authoritative execution environment.

- [x] **Step 3: Verify the Next.js M2 web application**

```bash
cd src/PeopleSyncD.Web
npm ci --no-fund
npm run typecheck
npm run build
```

Expected on a normal Linux host: all commands exit `0`. If the sandbox lacks `/proc`, require the GitHub Actions web job to pass.

- [x] **Step 4: Verify documentation and security contracts**

```bash
npx --yes markdownlint-cli2 "**/*.md" "#node_modules" "#**/node_modules/**"
python scripts/governance/validate_master_blueprint.py
if git grep -nE 'AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{36}|-----BEGIN ([A-Z ]+ )?PRIVATE KEY-----' -- . ':!*.lock'; then exit 1; fi
```

Expected: Markdown and blueprint validation exit `0`, and the secret-pattern scan returns no matches.

- [ ] **Step 5: Run externally hosted platform gates**

Require the pull request workflows for .NET 9 restore/format/build/test, PostgreSQL migrations, OpenAPI, Docker images and Compose, Codespaces live stack, persistent hosting, security, traceability, and Windows packaging.

### Task 4: Publish the consolidation for founder review

**Files:**

- No source changes.

**Interfaces:**

- Consumes: verified branch `agent/consolidate-peoplesyncd-baseline`.
- Produces: a draft pull request targeting `main`.

- [x] **Step 1: Inspect final scope**

```bash
git status --short --branch
git diff --stat origin/main...HEAD
git log --oneline origin/main..HEAD
```

- [ ] **Step 2: Push the consolidation branch**

```bash
git push -u origin agent/consolidate-peoplesyncd-baseline
```

- [ ] **Step 3: Open a draft pull request**

Create a draft PR titled `Consolidate PeopleSyncD authoritative baseline` targeting `main`. Describe the authoritative-tree decision, preserved histories, validation evidence, known environment limitations, and the explicit requirement for separate founder approval before merge.

- [ ] **Step 4: Inspect all triggered GitHub Actions**

Do not mark the consolidation ready or merge it until every required workflow passes or each failure has been diagnosed and corrected.

# CareSyncD Permanent Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver CareSyncD Permanent as a browser-native, installable, offline-capable clinical education simulator on a permanent GitHub Pages URL with v3 learner-experience parity and no PC or paid runtime dependency.

**Architecture:** Execute three gated plans in order: browser core/persistence, PWA interface/offline behavior, then dedicated-repository GitHub Pages deployment. Development is staged on `PeopleSyncD:caresyncd-pages` under `caresyncd-web/`; production is promoted to the root of a dedicated public `deaconjason-stack/CareSyncD` repository only after all staging gates pass.

**Tech Stack:** JavaScript ES modules, Node.js 22 built-in test runner, IndexedDB, HTML/CSS, Web App Manifest, Service Worker, GitHub Actions, GitHub Pages.

**Spec:** `docs/superpowers/specs/2026-09-22-caresyncd-permanent-design.md`

## Global Constraints

- Educational simulation only. Not for real-patient diagnosis, monitoring, or treatment.
- Preserve 8 clinical missions and 3 hospital-shift simulations.
- No backend is required for the initial permanent edition.
- No real-time cross-device classroom sync in this release.
- No PHI storage, EHR integration, medical-device claims, accreditation claims, or clinical-efficacy claims.
- Permanent production must not depend on a home PC, Cloudflare Quick Tunnel, Railway, Vercel Functions, or a long-running Node server.
- PeopleSyncD `main` remains untouched.
- Production project URL target: `https://deaconjason-stack.github.io/CareSyncD/`.
- Production repository contains no secrets or API keys and is public for no-cost GitHub Pages hosting.

## Authoritative Execution Order

1. `docs/superpowers/plans/2026-09-22-caresyncd-permanent-core.md`
2. `docs/superpowers/plans/2026-09-22-caresyncd-permanent-pwa.md`
3. `docs/superpowers/plans/2026-09-22-caresyncd-permanent-deploy.md`

Do not begin a later phase until the prior phase's exit criteria pass.

## Self-Review Corrections — These Override Conflicting Snippets in Phase Plans

### 1. Cross-platform Node test command

The package-level test script is authoritative as:

```json
"test": "node --test"
```

Use specific test filenames only for focused red/green steps. Do not use `node --test tests/**/*.test.mjs` as the package script because shell glob behavior differs across environments.

### 2. Exact v3 source extraction path

Before porting definitions or engines, extract the verified server release committed on the source branch:

```bash
python - <<'PY'
from pathlib import Path
import zipfile, shutil
src = Path('releases/CareSyncD-v3-HOSPITAL-SHIFT.zip')
out = Path('.caresyncd-v3-source')
if out.exists(): shutil.rmtree(out)
with zipfile.ZipFile(src) as z:
    assert z.testzip() is None
    z.extractall(out)
print(out / 'CareSyncD')
PY
```

Use these exact source files as the behavior reference:

- `.caresyncd-v3-source/CareSyncD/src/actions.js`
- `.caresyncd-v3-source/CareSyncD/src/scenarios.js`
- `.caresyncd-v3-source/CareSyncD/src/simulation.js`
- `.caresyncd-v3-source/CareSyncD/src/shift.js`
- `.caresyncd-v3-source/CareSyncD/tests/simulation.test.js`

Port CommonJS exports to browser ESM; do not copy Node filesystem/server dependencies into `caresyncd-web/src/`.

### 3. Exact reproducible PWA icon generation

Do not hand-create unexplained binary PNGs. Add `sharp` as a development-only build tool:

```bash
npm install --save-dev sharp
```

Create `icons/icon-source.svg` with this source shape:

```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" role="img" aria-label="CareSyncD">
  <rect width="512" height="512" rx="112" fill="#07131f"/>
  <path d="M126 256c0-72 58-130 130-130 45 0 85 23 108 58l-58 36c-11-17-30-28-50-28-35 0-64 29-64 64s29 64 64 64c23 0 44-12 55-32l60 32c-22 41-65 66-115 66-72 0-130-58-130-130Z" fill="#ffffff"/>
  <path d="M252 214h24v30h30v24h-30v30h-24v-30h-30v-24h30Z" fill="#ffffff"/>
</svg>
```

Create `scripts/generate-icons.mjs`:

```js
import sharp from 'sharp';
for (const size of [192, 512]) {
  await sharp('icons/icon-source.svg').resize(size, size).png().toFile(`icons/icon-${size}.png`);
}
```

Add:

```json
"icons": "node scripts/generate-icons.mjs"
```

Run `npm run icons`, commit the source SVG, generator, lockfile, and generated 192/512 PNGs. Manifest entries must use relative paths.

### 4. Current GitHub Pages action versions

Use the current GitHub-documented Pages action majors:

```yaml
- uses: actions/checkout@v6
- uses: actions/configure-pages@v5
- uses: actions/upload-pages-artifact@v4
- uses: actions/deploy-pages@v4
```

The deployment job requires `pages: write`, `id-token: write`, `needs: verify`, and the `github-pages` environment. These settings supersede older action versions in any phase snippet.

### 5. Public repository requirement for the no-cost path

For GitHub Free, use a public dedicated repository for this Pages deployment. No secret, API key, PHI, private customer data, or proprietary server credential may be committed to it.

## Review Focus

1. **Clinical parity drift during port:** compare browser behavior to the extracted v3 tests and source before accepting a rewritten rule.
2. **Persistence/data loss:** quota failures, corrupted imports, schema upgrades, and resume boundaries must preserve valid existing data.
3. **Pages subpath/404 regressions:** all local assets are relative and navigation is hash-based; production live verification must test the public URL itself.
4. **PWA update safety:** new service-worker code waits while a simulation is active.
5. **Public-repository hygiene:** secret scan and PHI-safe content check run before every production deploy.

## Spec Coverage Map

- Spec §§1–5, 12–17 → Phase 1 core plan.
- Spec §§6–11, 18–19 → Phase 2 PWA plan.
- Spec §§20–22 remain future seams/non-goals; Phase 1 adapter/event boundaries preserve them without implementing them.
- Spec §§23–24 → Phase 2 static checks plus Phase 3 Pages/CI/live verification.
- Spec §§25–27 → enforced as global constraints and final release gates across all phases.

## Master Verification Gate

At the end of Phase 3, the production repository must pass:

```bash
npm test
npm run check
npm run check:site
npm run check:production
node scripts/verify-live.mjs https://deaconjason-stack.github.io/CareSyncD/
```

GitHub Actions CI and Pages deployment for the same production HEAD must both be green. Then verify from a separate device/network with all development servers and tunnels stopped.

## Definition of Done

CareSyncD Permanent is done only when the public GitHub Pages URL loads independently of the user's PC; all 8 missions and 3 shifts remain playable; saved progress and active-shift resume work; the core app works offline after first load; the PWA is installable where platform-supported; updates do not interrupt an active simulation; production contains no secrets/PHI; and release evidence accurately states the remaining limitation that cross-device classroom synchronization is future work.

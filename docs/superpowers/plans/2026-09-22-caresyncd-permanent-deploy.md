# CareSyncD Permanent Deployment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Publish the verified CareSyncD Permanent PWA from its own dedicated GitHub repository and prove that the permanent HTTPS site, repository-subpath routing, PWA assets, offline shell, and deployment automation work without Jason's PC or a paid runtime.

**Architecture:** Keep `deaconjason-stack/PeopleSyncD:caresyncd-pages` as the staging/source-history branch during conversion, then create a dedicated public `deaconjason-stack/CareSyncD` repository for the production static site. Copy the verified `caresyncd-web/` subtree to that repository root, deploy with GitHub Pages Actions, and keep CI and Pages deployment as separate required jobs.

**Tech Stack:** GitHub repository, GitHub Actions, GitHub Pages, static PWA assets, Node.js 22 verification.

**Spec:** `docs/superpowers/specs/2026-09-22-caresyncd-permanent-design.md`

## Global Constraints

- The production site must not depend on a home PC, Cloudflare Quick Tunnel, Railway, Vercel Functions, or a Node server.
- Production repository must contain no secrets, API keys, PHI, or real patient data.
- The production Pages build is static and free-hostable; a public repository is the default no-cost configuration.
- PeopleSyncD `main` remains untouched by the CareSyncD deployment.
- The server-based v3 release remains preserved; Permanent is a separate browser-native deployment tier.
- Do not publish until Phase 1 and Phase 2 test/check commands pass with zero failures.
- All URLs and assets must work from `https://deaconjason-stack.github.io/CareSyncD/`.

## Review Focus

1. **Pages not enabled or wrong source:** deployment must fail visibly and provide the exact repository setting/API action required; never report success from a green CI job alone.
2. **Case-sensitive production paths:** Linux/Pages must resolve every JS/CSS/icon import with exact filename casing even if Windows development tolerated a mismatch.
3. **Stale service-worker cache:** release verification must prove the deployed build identifier changes and the old cache can be replaced after an active run ends.
4. **404 on deep navigation:** direct visits to the Pages root plus `#/...` routes must return the app shell; no pathname route may rely on server rewrites.
5. **Public repository exposure:** automated secret scan must reject common key/token patterns before publishing.

---

## File Structure in the dedicated production repository

At production cutover, contents of staging `caresyncd-web/` become repository root:

- `index.html`
- `manifest.webmanifest`
- `sw.js`
- `icons/`
- `styles/`
- `src/`
- `tests/`
- `scripts/`
- `package.json`
- `README.md`
- `LICENSE` only if explicitly chosen; do not invent licensing terms.
- `.github/workflows/ci.yml`
- `.github/workflows/pages.yml`

### Task 1: Add production-readiness checks to staging

**Files:**
- Create: `caresyncd-web/scripts/check-production.mjs`
- Create: `caresyncd-web/tests/deploy/production-readiness.test.mjs`
- Modify: `caresyncd-web/package.json`

**Interfaces:**
- Adds `npm run check:production`.

- [ ] **Step 1: Write the production-readiness tests**

Tests must fail if any text source contains a root-absolute local asset, localhost URL, `trycloudflare.com`, Railway/Vercel runtime URL, a common secret prefix (`sk-`, `ghp_`, `github_pat_`, `AIza`), or a reference to a missing local file.

- [ ] **Step 2: Add exact URL/base expectations**

The checker must model the production base URL as `https://deaconjason-stack.github.io/CareSyncD/` and assert:

```js
new URL('./src/main.js', base).href === 'https://deaconjason-stack.github.io/CareSyncD/src/main.js'
new URL('./manifest.webmanifest', base).href === 'https://deaconjason-stack.github.io/CareSyncD/manifest.webmanifest'
```

- [ ] **Step 3: Implement case-sensitive path validation**

Walk the repository using actual directory entries and compare each imported/referenced path segment exactly; do not normalize case.

- [ ] **Step 4: Update package scripts**

```json
"check:production": "node scripts/check-production.mjs"
```

- [ ] **Step 5: Run full staging gate**

Run:

```bash
npm test
npm run check
npm run check:site
npm run check:production
```

Expected: zero failures.

- [ ] **Step 6: Commit**

```bash
git add caresyncd-web/scripts/check-production.mjs caresyncd-web/tests/deploy/production-readiness.test.mjs caresyncd-web/package.json
git commit -m "test(caresyncd): add production publishing gate"
```

### Task 2: Create the dedicated CareSyncD production repository

**Files:**
- No product files changed in staging.
- Creates GitHub repository: `deaconjason-stack/CareSyncD`.

**Interfaces:**
- Produces production Git remote `https://github.com/deaconjason-stack/CareSyncD.git`.

- [ ] **Step 1: Verify the repository name is unused**

Run:

```bash
gh repo view deaconjason-stack/CareSyncD
```

Expected before creation: non-zero exit / repository not found. If it exists, inspect it and do not overwrite it blindly.

- [ ] **Step 2: Create a public repository with no generated starter files**

Run:

```bash
gh repo create deaconjason-stack/CareSyncD --public --description "CareSyncD Permanent — Clinical Judgment Under Pressure" --confirm
```

Expected: repository created. Do not initialize README/license remotely because the verified staging tree will supply repository content.

- [ ] **Step 3: Verify visibility and default URL**

Run:

```bash
gh repo view deaconjason-stack/CareSyncD --json nameWithOwner,visibility,url
```

Expected: `visibility` is `PUBLIC` and owner/name is exact.

- [ ] **Step 4: Stop if repository creation is unavailable**

If the execution environment lacks authenticated `gh` repository-admin capability, record the exact failure. The user must create an empty public repo named `CareSyncD`; after that, resume at Task 3. Do not fall back to PeopleSyncD Pages.

### Task 3: Promote the verified staging subtree to production root

**Files:**
- Copy all verified files from `PeopleSyncD:caresyncd-pages/caresyncd-web/` into the root of the dedicated CareSyncD repository.
- Create: `README.md` in production repo.

**Interfaces:**
- Production root is the same static app tested in staging.

- [ ] **Step 1: Create a clean work directory and copy only the verified subtree**

```bash
rm -rf /tmp/caresyncd-production
mkdir /tmp/caresyncd-production
cp -R caresyncd-web/. /tmp/caresyncd-production/
```

On PowerShell use equivalent `Copy-Item -Recurse`; do not copy PeopleSyncD solution files, release ZIPs, or unrelated docs.

- [ ] **Step 2: Create production README**

README must identify CareSyncD Permanent, list educational-simulation safety language, local static serving instructions, `npm test`/checks, and the production Pages URL. Do not claim accreditation, FDA status, or clinical validation.

- [ ] **Step 3: Initialize and push production repository**

```bash
cd /tmp/caresyncd-production
git init -b main
git remote add origin https://github.com/deaconjason-stack/CareSyncD.git
git add .
git commit -m "feat: publish CareSyncD Permanent foundation"
git push -u origin main
```

- [ ] **Step 4: Verify repository tree**

Run:

```bash
gh api repos/deaconjason-stack/CareSyncD/contents --jq '.[].name'
```

Expected: `index.html`, `manifest.webmanifest`, `sw.js`, `src`, `styles`, `icons`, `tests`, `scripts`, `package.json`, `README.md`; no PeopleSyncD solution files.

### Task 4: Add production CI workflow

**Files:**
- Create: `.github/workflows/ci.yml`

**Interfaces:**
- Runs on pushes/PRs to `main` and manual dispatch.

- [ ] **Step 1: Create CI workflow**

```yaml
name: CareSyncD CI
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]
  workflow_dispatch:
permissions:
  contents: read
jobs:
  verify:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
      - run: npm test
      - run: npm run check
      - run: npm run check:site
      - run: npm run check:production
```

- [ ] **Step 2: Push and inspect the workflow run**

Run:

```bash
git add .github/workflows/ci.yml
git commit -m "ci: verify CareSyncD Permanent"
git push
```

Expected: GitHub Actions `CareSyncD CI` completes successfully.

- [ ] **Step 3: Do not proceed on red CI**

Fetch failed job logs, fix the smallest concrete issue, rerun, and require green before Pages deployment.

### Task 5: Add GitHub Pages deployment workflow

**Files:**
- Create: `.github/workflows/pages.yml`

**Interfaces:**
- Deploys repository root to GitHub Pages after verification.

- [ ] **Step 1: Create Pages workflow**

```yaml
name: Deploy CareSyncD Permanent
on:
  push:
    branches: [main]
  workflow_dispatch:
permissions:
  contents: read
  pages: write
  id-token: write
concurrency:
  group: pages
  cancel-in-progress: false
jobs:
  verify:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
      - run: npm test
      - run: npm run check
      - run: npm run check:site
      - run: npm run check:production
  deploy:
    needs: verify
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/configure-pages@v5
      - uses: actions/upload-pages-artifact@v3
        with:
          path: '.'
      - name: Deploy
        id: deployment
        uses: actions/deploy-pages@v4
```

- [ ] **Step 2: Enable Pages build type `workflow`**

Preferred authenticated command:

```bash
gh api --method POST repos/deaconjason-stack/CareSyncD/pages -f build_type=workflow
```

If GitHub reports the Pages site already exists, inspect it instead of recreating. If repository-admin API access is unavailable, set **Settings → Pages → Build and deployment → Source: GitHub Actions** once, then continue.

- [ ] **Step 3: Push workflow**

```bash
git add .github/workflows/pages.yml
git commit -m "deploy: publish CareSyncD with GitHub Pages"
git push
```

Expected: verification job green, deploy job green, environment reports a `page_url`.

### Task 6: Verify the public production site, not merely the workflow

**Files:**
- Create: `scripts/verify-live.mjs`

**Interfaces:**
- `node scripts/verify-live.mjs https://deaconjason-stack.github.io/CareSyncD/` exits 0 only when required public resources are valid.

- [ ] **Step 1: Implement live verification script**

Fetch with redirects enabled:

- `/CareSyncD/` → HTTP 200 and body contains `CareSyncD` plus safety statement.
- `/CareSyncD/manifest.webmanifest` → HTTP 200 and valid JSON.
- `/CareSyncD/sw.js` → HTTP 200 JavaScript.
- `/CareSyncD/src/main.js` → HTTP 200.
- manifest `start_url` resolves under `/CareSyncD/`.
- every manifest icon resolves HTTP 200.

- [ ] **Step 2: Verify hash deep-link behavior**

Fetch `https://deaconjason-stack.github.io/CareSyncD/#/hospital-shifts` and `#/progress`; HTTP request must still return the root shell with 200 because routing occurs after `#` in the browser.

- [ ] **Step 3: Run the live verifier**

```bash
node scripts/verify-live.mjs https://deaconjason-stack.github.io/CareSyncD/
```

Expected: PASS for every resource. Do not announce the URL until this passes.

- [ ] **Step 4: Commit verifier**

```bash
git add scripts/verify-live.mjs
git commit -m "test: add live CareSyncD Pages verification"
git push
```

### Task 7: Verify PWA/offline/update behavior in a real browser

**Files:**
- No source change unless a defect is found.

**Interfaces:**
- Manual/browser acceptance evidence.

- [ ] **Step 1: First-load acceptance**

Open the production URL in a clean browser profile. Verify Home renders, catalog counts are 8 missions/3 shifts, safety notice is visible, and there are no console 404s.

- [ ] **Step 2: Installability acceptance**

Verify the browser detects manifest/service worker and offers install/add-to-home-screen where platform-supported. Launch standalone mode and verify navigation works.

- [ ] **Step 3: Persistence acceptance**

Start `hospital_day`, allow at least one timed event, perform an action, close the app, reopen, and verify `Resume Shift` restores the exact simulated clock/state rather than advancing by wall-clock downtime.

- [ ] **Step 4: Offline acceptance**

After first load, disable network and reload the installed/app tab. Verify the app shell, cached definitions, active run, clinical actions, task board, scoring, and debrief remain functional.

- [ ] **Step 5: Update acceptance**

Publish a harmless build-version increment while an active shift is open. Verify the UI reports `CareSyncD update available` and does not reload/activate the new worker until the shift ends or the learner explicitly exits it.

- [ ] **Step 6: Record evidence**

Add `docs/release/3.1.0-permanent-verification.md` with date, tested URL, commit SHA, CI run links/IDs, live verifier output, browsers/devices tested, offline result, resume result, and known limitations. Explicitly state cross-device instructor synchronization is not included.

### Task 8: Final production gate

**Files:**
- Create or update: `docs/release/3.1.0-permanent-verification.md`

- [ ] **Step 1: Re-run repository checks at production HEAD**

```bash
npm test
npm run check
npm run check:site
npm run check:production
node scripts/verify-live.mjs https://deaconjason-stack.github.io/CareSyncD/
```

Expected: all exit 0.

- [ ] **Step 2: Confirm GitHub Actions**

Both `CareSyncD CI` and `Deploy CareSyncD Permanent` for production HEAD must be successful.

- [ ] **Step 3: Confirm hosting independence**

Shut down any local development server/tunnel and load the production URL from a separate device/network. Expected: site remains available because GitHub Pages serves it.

- [ ] **Step 4: Tag the verified release**

```bash
git tag -a v3.1.0-permanent -m "CareSyncD Permanent v3.1.0"
git push origin v3.1.0-permanent
```

- [ ] **Step 5: Final release statement**

Only after every gate is green may the implementation be described as live. State accurately: CareSyncD Permanent is an educational browser/PWA simulation hosted on GitHub Pages; it has local persistence/offline capability; cross-device classroom sync and institution-grade backend features remain future work.

## Phase 3 Exit Criteria

Production is complete only when:

- Dedicated `deaconjason-stack/CareSyncD` repository exists and contains only CareSyncD production code/docs.
- CI is green at production HEAD.
- Pages deployment is green at production HEAD.
- `https://deaconjason-stack.github.io/CareSyncD/` returns the verified app publicly.
- Manifest, service worker, main module, CSS, and icons return 200 from production.
- The PWA can resume an active shift after close/reopen without simulated-time drift.
- Core simulation works offline after initial cache.
- A service-worker update waits during an active simulation.
- No home PC, tunnel, Railway, or Vercel runtime is needed for the site to stay available.
- Release evidence and `v3.1.0-permanent` tag are created.

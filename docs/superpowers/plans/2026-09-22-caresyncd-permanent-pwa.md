# CareSyncD Permanent PWA Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the installable, offline-capable CareSyncD Permanent learner interface on top of the verified browser core without adding a backend.

**Architecture:** Use semantic HTML, CSS, and native ES modules with hash-based routing so GitHub Pages subpath hosting works without rewrite rules. The UI speaks only to `SessionController` and read-only catalog/profile services; PWA caching is handled by a service worker that never activates a new build in the middle of an active simulation.

**Tech Stack:** HTML5, CSS, JavaScript ES modules, Web App Manifest, Service Worker, Node.js 22 test runner.

**Spec:** `docs/superpowers/specs/2026-09-22-caresyncd-permanent-design.md`

## Global Constraints

- Preserve the v3 learner experience; this is not a stripped-down demo.
- Keep the educational-simulation safety statement visible and accurate.
- Use hash routing or another static-safe navigation strategy; no server rewrite dependency.
- All asset references must remain relative and repository-subpath safe.
- Core simulation must work offline after first successful load.
- Do not activate an app update while an active simulation exists.
- Mobile must use touch-friendly panels; desktop may use a sidebar/dashboard layout.
- No PHI collection fields or copy that invites real-patient information.

## Review Focus

1. **Repository subpath hosting:** every route, asset, manifest icon, and service-worker URL must work from `/PeopleSyncD/` during staging and from `/CareSyncD/` after repository separation.
2. **Small-screen pressure:** the simulation cockpit must not hide urgent patient/task information behind unreachable desktop-only layout.
3. **Keyboard and screen-reader access:** all action buttons, navigation, tabs/panels, alerts, and dialogs must have semantic controls and focus behavior.
4. **Offline first reload:** after one successful online load, the app shell, scenario definitions, and saved active run must remain usable with network unavailable.
5. **Update during active shift:** a newly installed service worker must wait; it must not replace code until the active run completes or the learner explicitly exits it.

---

## File Structure

- `caresyncd-web/index.html` — app shell and safety copy.
- `caresyncd-web/styles/tokens.css` — spacing/type/semantic urgency variables.
- `caresyncd-web/styles/app.css` — responsive application layout.
- `caresyncd-web/src/ui/router.js` — hash routing.
- `caresyncd-web/src/ui/app-shell.js` — nav/header/offline/update indicators.
- `caresyncd-web/src/ui/views/home-view.js` — resume/start/profile summary.
- `caresyncd-web/src/ui/views/catalog-view.js` — mission/shift cards.
- `caresyncd-web/src/ui/views/simulation-view.js` — cockpit composition.
- `caresyncd-web/src/ui/views/progress-view.js` — history/competencies/achievements.
- `caresyncd-web/src/ui/views/debrief-view.js` — structured debrief.
- `caresyncd-web/src/ui/views/instructor-view.js` — local pause/resume/end controls.
- `caresyncd-web/src/ui/views/settings-view.js` — preferences and backup/restore.
- `caresyncd-web/src/ui/components/patient-strip.js` — patient selector/status.
- `caresyncd-web/src/ui/components/task-board.js` — priorities, due states, delegation.
- `caresyncd-web/src/ui/components/clinical-workspace.js` — vitals/chart/labs/orders/notes.
- `caresyncd-web/src/ui/components/action-center.js` — grouped actions.
- `caresyncd-web/src/ui/components/shift-ribbon.js` — clock/scores/open/missed/next due.
- `caresyncd-web/src/ui/components/live-region.js` — accessible nonvisual urgent updates.
- `caresyncd-web/src/main.js` — bootstraps persistence, controller, router, PWA hooks.
- `caresyncd-web/manifest.webmanifest` — install metadata.
- `caresyncd-web/sw.js` — offline caching/update lifecycle.
- `caresyncd-web/icons/` — PWA icons.
- `caresyncd-web/tests/ui/**` — route/presentation tests.
- `caresyncd-web/tests/pwa/**` — manifest/service-worker tests.

### Task 1: Build a subpath-safe app shell and router

**Files:**
- Create: `caresyncd-web/index.html`
- Create: `caresyncd-web/styles/tokens.css`
- Create: `caresyncd-web/styles/app.css`
- Create: `caresyncd-web/src/ui/router.js`
- Create: `caresyncd-web/src/ui/app-shell.js`
- Create: `caresyncd-web/tests/ui/router.test.mjs`

**Interfaces:**
- Produces: `createRouter({ onRoute })`, `parseHash(hash) -> { name, params }`, `navigate(name, params)`.
- Produces: `renderAppShell({ route, offline, updateAvailable, content }) -> string|Node` according to implementation style.

- [ ] **Step 1: Write failing route tests**

```js
import test from 'node:test';
import assert from 'node:assert/strict';
import { parseHash } from '../../src/ui/router.js';

test('routes do not depend on server rewrites', () => {
  assert.deepEqual(parseHash('#/home'), { name: 'home', params: {} });
  assert.deepEqual(parseHash('#/simulation/run-12'), { name: 'simulation', params: { id: 'run-12' } });
});

test('unknown routes fall back to home', () => {
  assert.deepEqual(parseHash('#/does-not-exist'), { name: 'home', params: {} });
});
```

- [ ] **Step 2: Run and verify failure**

Run: `node --test tests/ui/router.test.mjs`
Expected: FAIL because router module is absent.

- [ ] **Step 3: Implement semantic HTML shell and hash router**

`index.html` must use relative references such as `./styles/app.css` and `./src/main.js`, include viewport/theme metadata, and display the safety statement in the shell/footer. Do not use `<base href="/">`.

- [ ] **Step 4: Add app-shell accessibility requirements**

Use landmarks (`header`, `nav`, `main`), a skip link, real `<button>`/`<a>` controls, `aria-current="page"` for active navigation, and a visible offline/update status area.

- [ ] **Step 5: Run tests**

Run: `npm test`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add caresyncd-web/index.html caresyncd-web/styles caresyncd-web/src/ui/router.js caresyncd-web/src/ui/app-shell.js caresyncd-web/tests/ui/router.test.mjs
git commit -m "feat(caresyncd): add static-safe application shell"
```

### Task 2: Implement Home, catalogs, and profile summary

**Files:**
- Create: `caresyncd-web/src/ui/views/home-view.js`
- Create: `caresyncd-web/src/ui/views/catalog-view.js`
- Create: `caresyncd-web/tests/ui/home-view.test.mjs`

**Interfaces:**
- Consumes: catalog definitions, learner profile, active-run summary.
- Produces: `homeViewModel({ profile, activeRun, recentRuns })` and `catalogViewModel({ kind, definitions })` pure presentation models.

- [ ] **Step 1: Write pure view-model tests**

```js
const model = homeViewModel({
  profile: { displayName: 'Learner', rank: 'Rapid Responder', xp: 320, achievements: ['Situational Awareness'] },
  activeRun: { runId: 'r1', definitionId: 'hospital_day', kind: 'shift' },
  recentRuns: []
});
assert.equal(model.primaryAction.label, 'Resume Shift');
assert.equal(model.rank, 'Rapid Responder');
```

- [ ] **Step 2: Run and verify failure**

Run: `node --test tests/ui/home-view.test.mjs`
Expected: FAIL.

- [ ] **Step 3: Implement home and catalog renderers**

Home must expose resume first when an active run exists, then Start Hospital Shift and Start Clinical Mission. Catalog cards must show title, level, difficulty options, and for shifts the starting/potential patient count plus event count without exposing hidden answers.

- [ ] **Step 4: Test no-active-run and empty-history states**

Expected copy must remain useful rather than blank: first-time users see clear start actions and zero-state progress text.

- [ ] **Step 5: Commit**

```bash
git add caresyncd-web/src/ui/views/home-view.js caresyncd-web/src/ui/views/catalog-view.js caresyncd-web/tests/ui/home-view.test.mjs
git commit -m "feat(caresyncd): add learner home and catalogs"
```

### Task 3: Implement the responsive simulation cockpit

**Files:**
- Create: `caresyncd-web/src/ui/views/simulation-view.js`
- Create: `caresyncd-web/src/ui/components/patient-strip.js`
- Create: `caresyncd-web/src/ui/components/clinical-workspace.js`
- Create: `caresyncd-web/src/ui/components/action-center.js`
- Create: `caresyncd-web/src/ui/components/task-board.js`
- Create: `caresyncd-web/src/ui/components/shift-ribbon.js`
- Create: `caresyncd-web/src/ui/components/live-region.js`
- Create: `caresyncd-web/tests/ui/simulation-view.test.mjs`

**Interfaces:**
- Consumes: `SessionController.getActiveSnapshot()` and dispatch callback.
- Produces: `simulationViewModel(snapshot)` and event handlers that emit controller commands only.

- [ ] **Step 1: Write view-model tests for scenario and shift modes**

Assert a single-patient scenario has no shift-task panel; a shift exposes patient strip, shift clock, clinical score, operations score, open/missed/next-due metrics, staff/delegation options, and handoff tab.

- [ ] **Step 2: Add urgency-semantic tests**

```js
assert.equal(urgencyForPatient({ status: 'critical' }), 'critical');
assert.equal(urgencyForTask({ status: 'missed' }), 'overdue');
```

Urgency must be represented by text/icon/ARIA label as well as visual styling; color alone is insufficient.

- [ ] **Step 3: Implement cockpit components**

Desktop composition: patient strip → shift ribbon → clinical workspace/action center/task board. Mobile: sticky patient selector and urgency summary; panels switch with real buttons/tabs and retain scroll/focus safely.

- [ ] **Step 4: Implement dispatch-only interactions**

Actions call controller commands such as `{ type: 'clinical-action', actionId, patientId }`, `{ type: 'task-command', taskId, command: 'delegate', staffId }`, `{ type: 'tick', minutes: 1 }`; components never mutate engine state directly.

- [ ] **Step 5: Add accessible live announcements**

`live-region.js` must announce critical patient transitions, new admissions, new call lights, and newly missed tasks using a polite/assertive strategy that avoids repeating every timer tick.

- [ ] **Step 6: Run tests and commit**

Run: `npm test`
Expected: PASS.

```bash
git add caresyncd-web/src/ui/views/simulation-view.js caresyncd-web/src/ui/components caresyncd-web/tests/ui/simulation-view.test.mjs caresyncd-web/styles
git commit -m "feat(caresyncd): add responsive clinical cockpit"
```

### Task 4: Implement debrief, progress, and achievements

**Files:**
- Create: `caresyncd-web/src/ui/views/debrief-view.js`
- Create: `caresyncd-web/src/ui/views/progress-view.js`
- Create: `caresyncd-web/tests/ui/debrief-progress.test.mjs`

**Interfaces:**
- Produces: `debriefViewModel(run)` and `progressViewModel({ profile, runs })`.

- [ ] **Step 1: Write debrief tests**

Assert the model contains timeline, evidence-linked strengths, improvement opportunities, patient outcomes, operational metrics, seven competency domains, final score, achievements, and Retry/Try Harder/Next actions.

- [ ] **Step 2: Write progress tests**

Test first-run zero state, multiple historical definition versions, rank/XP, best/recent score, achievement count, and competency aggregation.

- [ ] **Step 3: Implement the views**

Historical runs must display the stored definition version, not silently relabel old results with the current catalog version.

- [ ] **Step 4: Run and commit**

Run: `npm test`
Expected: PASS.

```bash
git add caresyncd-web/src/ui/views/debrief-view.js caresyncd-web/src/ui/views/progress-view.js caresyncd-web/tests/ui/debrief-progress.test.mjs
git commit -m "feat(caresyncd): add debrief and progress views"
```

### Task 5: Implement local instructor controls and settings/backup UI

**Files:**
- Create: `caresyncd-web/src/ui/views/instructor-view.js`
- Create: `caresyncd-web/src/ui/views/settings-view.js`
- Create: `caresyncd-web/tests/ui/settings-instructor.test.mjs`

**Interfaces:**
- Instructor emits controller commands `pause`, `resume`, `finish` for the locally active session.
- Settings consumes `exportBackup()`/`importBackup()` and preference storage helpers.

- [ ] **Step 1: Write tests for instructor availability**

No active simulation → controls disabled with explanatory copy. Active simulation → pause/resume/end available. No copy may imply remote control of another device.

- [ ] **Step 2: Write settings tests**

Settings model must include text size, sound, reduced motion, backup export, restore import, install guidance/status, and the explicit “Never enter PHI or real patient identifiers” notice.

- [ ] **Step 3: Implement safe import confirmation flow**

The UI reads the selected JSON as text, calls `importBackup`, and shows success/failure only after validation completes. A failed import leaves current data unchanged.

- [ ] **Step 4: Run and commit**

Run: `npm test`
Expected: PASS.

```bash
git add caresyncd-web/src/ui/views/instructor-view.js caresyncd-web/src/ui/views/settings-view.js caresyncd-web/tests/ui/settings-instructor.test.mjs
git commit -m "feat(caresyncd): add local instructor and settings tools"
```

### Task 6: Bootstrap the application in `main.js`

**Files:**
- Create: `caresyncd-web/src/main.js`
- Create: `caresyncd-web/tests/ui/bootstrap.test.mjs`

**Interfaces:**
- `bootCareSyncD({ document, window, indexedDBFactory }) -> Promise<AppHandle>`.
- `AppHandle` exposes `render()`, `destroy()`, `controller`, and `router` for testability.

- [ ] **Step 1: Write bootstrap tests with injected environment**

Verify boot opens persistence, resumes an active run if present, starts the router, renders home by default, and reports persistence-unavailable state instead of crashing when IndexedDB cannot open.

- [ ] **Step 2: Implement boot wiring**

Instantiate `IndexedDbAdapter`, `EventBus`, `SessionController`, router, and shell. Keep browser globals at this boundary rather than inside engine modules.

- [ ] **Step 3: Run and commit**

Run: `npm test`
Expected: PASS.

```bash
git add caresyncd-web/src/main.js caresyncd-web/tests/ui/bootstrap.test.mjs
git commit -m "feat(caresyncd): wire permanent browser app"
```

### Task 7: Add manifest and installable PWA metadata

**Files:**
- Create: `caresyncd-web/manifest.webmanifest`
- Create: `caresyncd-web/icons/icon-192.png`
- Create: `caresyncd-web/icons/icon-512.png`
- Create: `caresyncd-web/tests/pwa/manifest.test.mjs`

**Interfaces:**
- Static metadata only.

- [ ] **Step 1: Write manifest tests**

Read the manifest in Node and assert `name`, `short_name`, `start_url: './#/home'`, `scope: './'`, `display: 'standalone'`, theme/background colors present, and 192/512 icons use relative paths.

- [ ] **Step 2: Run and verify failure**

Run: `node --test tests/pwa/manifest.test.mjs`
Expected: FAIL because manifest is absent.

- [ ] **Step 3: Create the manifest and icons**

The icon must be a CareSyncD-specific mark, not Ellie branding and not PeopleSyncD branding. Add `<link rel="manifest" href="./manifest.webmanifest">` to `index.html`.

- [ ] **Step 4: Run and commit**

Run: `npm test`
Expected: PASS.

```bash
git add caresyncd-web/manifest.webmanifest caresyncd-web/icons caresyncd-web/index.html caresyncd-web/tests/pwa/manifest.test.mjs
git commit -m "feat(caresyncd): add installable PWA metadata"
```

### Task 8: Add service worker with safe update lifecycle

**Files:**
- Create: `caresyncd-web/sw.js`
- Create: `caresyncd-web/src/pwa/update-manager.js`
- Create: `caresyncd-web/tests/pwa/service-worker.test.mjs`

**Interfaces:**
- `registerCareSyncDServiceWorker({ navigator, hasActiveRun, onUpdateAvailable })`.
- Service worker caches `CACHE_VERSION` and an explicit relative `APP_SHELL` array.

- [ ] **Step 1: Write static service-worker tests**

Assert `APP_SHELL` contains `./`, `./index.html`, CSS, manifest, main module, core data/engine modules, and icons; reject any cached path beginning with `/`.

- [ ] **Step 2: Write update-manager tests**

When `hasActiveRun()` is true, a waiting worker triggers `onUpdateAvailable` but does not receive `SKIP_WAITING`. When false and user accepts update, it may receive `SKIP_WAITING` and reload after `controllerchange`.

- [ ] **Step 3: Implement cache-first app shell and network-safe navigation**

For same-origin static assets use cache-first with versioned cache. For navigation requests return cached `index.html` when offline. Never cache arbitrary cross-origin requests or user backup files.

- [ ] **Step 4: Add offline status handling**

The app shell listens to `online`/`offline` and shows status without blocking simulation controls.

- [ ] **Step 5: Run and commit**

Run: `npm test`
Expected: PASS.

```bash
git add caresyncd-web/sw.js caresyncd-web/src/pwa caresyncd-web/tests/pwa caresyncd-web/src/main.js
git commit -m "feat(caresyncd): add safe offline service worker"
```

### Task 9: Add static/PWA acceptance checks

**Files:**
- Create: `caresyncd-web/scripts/check-static-site.mjs`
- Create: `caresyncd-web/tests/acceptance/pwa-parity.test.mjs`
- Modify: `caresyncd-web/package.json`

**Interfaces:**
- Adds `npm run check:site`.

- [ ] **Step 1: Add `check:site` script**

```json
"check:site": "node scripts/check-static-site.mjs"
```

- [ ] **Step 2: Implement static checker**

Parse `index.html`, manifest, and service worker text. Fail on root-absolute local assets (`src="/`, `href="/`, `'/...` cache entries), missing safety statement, missing manifest link, missing service-worker registration, or missing required icon files.

- [ ] **Step 3: Add acceptance test**

Using `MemoryAdapter`, boot a learner flow programmatically: start `hospital_day`, advance to an admission, dispatch an action, save, recreate controller, resume, finish, and assert a debrief/progress record can be rendered.

- [ ] **Step 4: Run Phase 2 verification**

Run:

```bash
npm test
npm run check
npm run check:site
```

Expected: zero failures.

- [ ] **Step 5: Commit**

```bash
git add caresyncd-web/package.json caresyncd-web/scripts/check-static-site.mjs caresyncd-web/tests/acceptance/pwa-parity.test.mjs
git commit -m "test(caresyncd): lock permanent PWA behavior"
```

## Phase 2 Exit Criteria

Do not begin production publishing until:

- Home, catalogs, simulation cockpit, progress, debriefs, instructor, and settings are functional.
- Mobile and desktop layouts expose the same urgent clinical/operational information.
- All interactive controls are keyboard reachable and semantically labeled.
- Manifest passes installability metadata checks.
- Service worker uses only relative/subpath-safe assets.
- First-load-online → subsequent-offline core app path is implemented.
- Updates wait during active simulations.
- Backup/restore is reachable through Settings and preserves data on rejection.
- `npm test`, `npm run check`, and `npm run check:site` all pass.

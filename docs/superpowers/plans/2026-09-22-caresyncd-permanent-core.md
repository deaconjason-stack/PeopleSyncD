# CareSyncD Permanent Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port the tested CareSyncD v3 simulation rules into browser-native ES modules with durable local persistence, autosave/resume, backup/restore, and no server dependency.

**Architecture:** Build a framework-free browser core under `caresyncd-web/` using native ES modules so the same files run in browsers and Node 22 tests. Keep simulation, shift orchestration, scoring, events, and persistence behind focused interfaces; the UI consumes a `SessionController` rather than reaching into engines or IndexedDB directly.

**Tech Stack:** JavaScript ES modules, Node.js 22 built-in test runner, browser IndexedDB, localStorage only for lightweight settings.

**Spec:** `docs/superpowers/specs/2026-09-22-caresyncd-permanent-design.md`

## Global Constraints

- Educational simulation only. Not for real-patient diagnosis, monitoring, or treatment.
- Preserve all 8 clinical missions and all 3 hospital-shift simulations from v3.
- No backend is required for the initial permanent edition.
- Do not add authentication, billing, EHR connectivity, PHI storage, LMS grade passback, or real-time cross-device classroom sync.
- All app records include `schemaVersion`.
- The rest of the app must not access IndexedDB directly; use `PersistenceAdapter`.
- Preserve v3 scenario identifiers, scoring semantics, event behavior, and debrief meaning unless an incompatibility is documented.
- The permanent build must work from a repository subpath and must not assume `/` hosting.
- Use simulated data only; never solicit PHI or real patient identifiers.

## Review Focus

1. **Storage quota/unavailable IndexedDB:** starting or autosaving a simulation must fail safely with a clear local-storage error state rather than losing an active run.
2. **Corrupt imported backup:** validation must reject malformed JSON or unsupported schema versions without overwriting existing learner data.
3. **Clock resume after long closure:** resume must restore the saved simulated elapsed time rather than applying real-world downtime as additional clinical deterioration.
4. **Definition upgrades:** historical runs must retain their original `definitionVersion` even when a newer scenario definition becomes current.
5. **Duplicate event processing:** repeated ticks or resume boundaries must not fire the same scheduled admission/order/interruption twice.

---

## File Structure

Create the permanent core under `caresyncd-web/` so it is isolated from PeopleSyncD during staging and can later become the root of a dedicated CareSyncD repository.

- `caresyncd-web/package.json` — Node 22 test/check scripts and ESM declaration.
- `caresyncd-web/src/data/actions.js` — canonical action catalog.
- `caresyncd-web/src/data/scenarios.js` — 8 versioned mission definitions.
- `caresyncd-web/src/data/shifts.js` — 3 versioned hospital-shift definitions.
- `caresyncd-web/src/domain/schema.js` — schema constants and record constructors.
- `caresyncd-web/src/engine/event-bus.js` — local event publisher/subscriber.
- `caresyncd-web/src/engine/scoring-engine.js` — domain, clinical, operations, XP, achievements.
- `caresyncd-web/src/engine/simulation-engine.js` — single-patient scenario runtime.
- `caresyncd-web/src/engine/shift-engine.js` — multi-patient runtime, tasks, admissions, staffing, delegation, handoff.
- `caresyncd-web/src/persistence/persistence-adapter.js` — persistence contract documentation/base helpers.
- `caresyncd-web/src/persistence/memory-adapter.js` — deterministic adapter for tests.
- `caresyncd-web/src/persistence/indexeddb-adapter.js` — browser persistence implementation.
- `caresyncd-web/src/persistence/migrations.js` — schema migrations.
- `caresyncd-web/src/persistence/backup.js` — JSON export/import validation.
- `caresyncd-web/src/app/session-controller.js` — starts, resumes, autosaves, and completes runs.
- `caresyncd-web/tests/**` — Node built-in tests by responsibility.

### Task 1: Establish the browser-core package and schema contracts

**Files:**
- Create: `caresyncd-web/package.json`
- Create: `caresyncd-web/src/domain/schema.js`
- Create: `caresyncd-web/tests/domain/schema.test.mjs`

**Interfaces:**
- Produces: `SCHEMA_VERSION`, `createLearnerProfile(input)`, `createSimulationRun(input)`, `assertSimulationRun(record)`.

- [ ] **Step 1: Write the failing schema tests**

```js
import test from 'node:test';
import assert from 'node:assert/strict';
import { SCHEMA_VERSION, createSimulationRun, assertSimulationRun } from '../../src/domain/schema.js';

test('new runs are versioned and retain definition version', () => {
  const run = createSimulationRun({
    runId: 'run-1', kind: 'scenario', definitionId: 'hypoxia', definitionVersion: '3.0.0',
    difficulty: 'standard', startTime: '2026-09-22T12:00:00.000Z'
  });
  assert.equal(run.schemaVersion, SCHEMA_VERSION);
  assert.equal(run.definitionVersion, '3.0.0');
  assert.equal(run.status, 'active');
});

test('invalid runs are rejected', () => {
  assert.throws(() => assertSimulationRun({ schemaVersion: 1, runId: '' }), /runId/);
});
```

- [ ] **Step 2: Run the test and verify failure**

Run: `cd caresyncd-web && node --test tests/domain/schema.test.mjs`
Expected: FAIL because `src/domain/schema.js` does not exist.

- [ ] **Step 3: Implement the package and schema module**

`package.json`:

```json
{
  "name": "caresyncd-permanent",
  "version": "3.1.0",
  "private": true,
  "type": "module",
  "engines": { "node": ">=22" },
  "scripts": {
    "test": "node --test tests/**/*.test.mjs",
    "check": "node scripts/check-project.mjs"
  }
}
```

`schema.js` must export `SCHEMA_VERSION = 1`; constructors must create complete default arrays/objects and `assertSimulationRun` must reject missing runId, invalid kind, missing definitionId/version, invalid status, and missing schemaVersion.

- [ ] **Step 4: Run the test and verify pass**

Run: `npm test`
Expected: PASS for schema tests.

- [ ] **Step 5: Commit**

```bash
git add caresyncd-web/package.json caresyncd-web/src/domain/schema.js caresyncd-web/tests/domain/schema.test.mjs
git commit -m "feat(caresyncd): establish browser core schema"
```

### Task 2: Port and validate v3 action/scenario/shift definitions

**Files:**
- Create: `caresyncd-web/src/data/actions.js`
- Create: `caresyncd-web/src/data/scenarios.js`
- Create: `caresyncd-web/src/data/shifts.js`
- Create: `caresyncd-web/tests/data/catalog.test.mjs`

**Interfaces:**
- Produces: `ACTIONS`, `SCENARIOS`, `SHIFTS`, `getScenario(id)`, `getShift(id)`.

- [ ] **Step 1: Write catalog tests**

```js
import test from 'node:test';
import assert from 'node:assert/strict';
import { SCENARIOS, getScenario } from '../../src/data/scenarios.js';
import { SHIFTS, getShift } from '../../src/data/shifts.js';

test('catalog preserves eight missions and three hospital shifts', () => {
  assert.equal(SCENARIOS.length, 8);
  assert.equal(SHIFTS.length, 3);
  assert.ok(getScenario('hypoxia'));
  assert.ok(getShift('hospital_day'));
});

test('every definition has an explicit version', () => {
  for (const item of [...SCENARIOS, ...SHIFTS]) assert.match(item.version, /^\d+\.\d+\.\d+$/);
});
```

- [ ] **Step 2: Run and verify failure**

Run: `node --test tests/data/catalog.test.mjs`
Expected: FAIL because data modules are absent.

- [ ] **Step 3: Port definitions from the verified v3 release**

Use the v3 identifiers exactly: scenarios include `hypoxia`, dehydration/hypotension, hypoglycemia, sepsis, stroke, anaphylaxis, opioid respiratory depression, and cardiac arrest; shifts include `med_surg`, `high_acuity`, and `hospital_day`. Represent definitions as plain frozen objects with explicit `version: '3.0.0'` and no Node-only APIs.

- [ ] **Step 4: Add integrity assertions**

Each shift must reference only valid scenario IDs, every linked task action must exist in `ACTIONS`, every scheduled event needs a stable `eventId`, and every scenario must list `expectedActions` and `criticalActions` arrays.

- [ ] **Step 5: Run tests**

Run: `npm test`
Expected: catalog tests PASS.

- [ ] **Step 6: Commit**

```bash
git add caresyncd-web/src/data caresyncd-web/tests/data
git commit -m "feat(caresyncd): port v3 simulation definitions"
```

### Task 3: Implement EventBus and ScoringEngine

**Files:**
- Create: `caresyncd-web/src/engine/event-bus.js`
- Create: `caresyncd-web/src/engine/scoring-engine.js`
- Create: `caresyncd-web/tests/engine/event-bus.test.mjs`
- Create: `caresyncd-web/tests/engine/scoring-engine.test.mjs`

**Interfaces:**
- Produces: `new EventBus()`, `subscribe(type, listener) -> unsubscribe`, `publish(type, payload)`.
- Produces: `calculateClinicalScore(session)`, `calculateOperationsScore(shift)`, `calculateOverallShiftScore(clinical, operations)`, `deriveAchievements(summary)`.

- [ ] **Step 1: Write failing tests for publish/unsubscribe and scoring**

```js
const bus = new EventBus();
let count = 0;
const off = bus.subscribe('state', () => count++);
bus.publish('state', {}); off(); bus.publish('state', {});
assert.equal(count, 1);
assert.equal(calculateOverallShiftScore(80, 100), 85);
```

- [ ] **Step 2: Run and verify failure**

Run: `node --test tests/engine/event-bus.test.mjs tests/engine/scoring-engine.test.mjs`
Expected: FAIL with missing modules.

- [ ] **Step 3: Implement minimal focused modules**

`EventBus` stores listeners in `Map<string, Set<Function>>`. Scoring preserves v3 weighting: `Math.round(clinical * 0.76 + operations * 0.24)`. Achievement derivation must include the v3 shift achievements when their predicates are met: `Shift Commander`, `Situational Awareness`, `Operational Excellence`, `Nothing Fell Through`.

- [ ] **Step 4: Run tests**

Run: `npm test`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add caresyncd-web/src/engine/event-bus.js caresyncd-web/src/engine/scoring-engine.js caresyncd-web/tests/engine
git commit -m "feat(caresyncd): add events and scoring core"
```

### Task 4: Port the single-patient SimulationEngine

**Files:**
- Create: `caresyncd-web/src/engine/simulation-engine.js`
- Create: `caresyncd-web/tests/engine/simulation-engine.test.mjs`

**Interfaces:**
- Consumes: `getScenario(id)`, `ACTIONS`, `EventBus`.
- Produces: `SimulationEngine.create({ definition, difficulty, learnerName, now })`, `tick(minutes)`, `performAction(actionId)`, `requestHint()`, `addNote(text)`, `snapshot()`.

- [ ] **Step 1: Write failing behavioral tests**

Cover hypoxia improvement after airway+oxygen, sequence feedback, guided hints, challenge-mode hint rejection, cardiac arrest CPR/defibrillation pulse restoration, and surprise event firing exactly once.

Example:

```js
const engine = SimulationEngine.create({ definition: getScenario('hypoxia'), difficulty: 'standard', learnerName: 'Test' });
const before = engine.snapshot().patient.vitals.spo2;
engine.performAction('airway');
engine.performAction('oxygen');
assert.ok(engine.snapshot().patient.vitals.spo2 > before);
```

- [ ] **Step 2: Run tests and verify failure**

Run: `node --test tests/engine/simulation-engine.test.mjs`
Expected: FAIL because engine module is absent.

- [ ] **Step 3: Port v3 state transitions without Node dependencies**

Keep all timing in simulated minutes. Maintain `processedEventIds: Set<string>` so repeated calls to `tick()` never duplicate a scheduled event. `snapshot()` returns JSON-safe data only; do not expose mutable internal objects.

- [ ] **Step 4: Add the resume-boundary duplicate-event test**

Serialize a snapshot immediately after an event, restore it, tick again, and assert the event count remains one.

- [ ] **Step 5: Run tests**

Run: `npm test`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add caresyncd-web/src/engine/simulation-engine.js caresyncd-web/tests/engine/simulation-engine.test.mjs
git commit -m "feat(caresyncd): port browser simulation engine"
```

### Task 5: Port the multi-patient ShiftEngine

**Files:**
- Create: `caresyncd-web/src/engine/shift-engine.js`
- Create: `caresyncd-web/tests/engine/shift-engine.test.mjs`

**Interfaces:**
- Consumes: `SimulationEngine`, `getShift`, scoring functions.
- Produces: `ShiftEngine.create({ definition, difficulty, learnerName })`, `tick(minutes)`, `selectPatient(id)`, `performAction(actionId, patientId)`, `taskCommand(taskId, command, staffId)`, `addNote(patientId, text)`, `snapshot()`, `handoffSummaries()`.

- [ ] **Step 1: Write failing tests for v3 shift behavior**

Tests must cover compressed clock, dynamic sepsis admission in `hospital_day`, staff delegation eligibility, delegated completion, missed timed task reducing operations score, clinical action auto-closing linked task, staffing unavailable/return transitions, and handoff generation.

- [ ] **Step 2: Run and verify failure**

Run: `node --test tests/engine/shift-engine.test.mjs`
Expected: FAIL with missing module.

- [ ] **Step 3: Implement shift orchestration**

Use stable task IDs and event IDs. Keep v3 staff roles and availability semantics. `taskCommand(..., 'delegate', staffId)` must reject wrong-role or unavailable staff. Missed tasks apply only their defined simulated consequences and only once.

- [ ] **Step 4: Add the long-closure resume invariant test**

Create a shift at simulated 08:15, serialize it, restore it with a wall clock many hours later, and assert simulated time is still 08:15 until `tick()` is explicitly called.

- [ ] **Step 5: Run tests**

Run: `npm test`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add caresyncd-web/src/engine/shift-engine.js caresyncd-web/tests/engine/shift-engine.test.mjs
git commit -m "feat(caresyncd): port hospital shift engine"
```

### Task 6: Implement persistence contract, memory adapter, migrations, and IndexedDB adapter

**Files:**
- Create: `caresyncd-web/src/persistence/persistence-adapter.js`
- Create: `caresyncd-web/src/persistence/memory-adapter.js`
- Create: `caresyncd-web/src/persistence/indexeddb-adapter.js`
- Create: `caresyncd-web/src/persistence/migrations.js`
- Create: `caresyncd-web/tests/persistence/persistence.test.mjs`

**Interfaces:**
- Produces adapter methods: `open()`, `getProfile(id)`, `putProfile(profile)`, `getActiveRun()`, `putActiveRun(run)`, `clearActiveRun()`, `listRuns()`, `putRun(run)`, `getMeta(key)`, `putMeta(key, value)`.
- Produces: `migrateRecord(record, targetVersion = SCHEMA_VERSION)`.

- [ ] **Step 1: Write the adapter contract tests against MemoryAdapter**

Test profile round trip, active run round trip, completed run list, clear active run, immutable copies, and schema migration.

- [ ] **Step 2: Add failure-mode tests**

Create a stub adapter that throws `QuotaExceededError` from `putActiveRun()` and assert callers receive a normalized `{ code: 'PERSISTENCE_UNAVAILABLE' }` error rather than silent success.

- [ ] **Step 3: Run and verify failure**

Run: `node --test tests/persistence/persistence.test.mjs`
Expected: FAIL.

- [ ] **Step 4: Implement MemoryAdapter and migrations**

Migrations must be pure functions. Unknown future schema versions must throw `UNSUPPORTED_SCHEMA_VERSION` rather than guessing.

- [ ] **Step 5: Implement IndexedDbAdapter with dependency injection**

Constructor signature: `new IndexedDbAdapter({ indexedDBFactory = globalThis.indexedDB, dbName = 'caresyncd-permanent' } = {})`. Use object stores `profiles`, `runs`, `active`, and `meta`. Store only JSON-safe records.

- [ ] **Step 6: Run tests**

Run: `npm test`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add caresyncd-web/src/persistence caresyncd-web/tests/persistence
git commit -m "feat(caresyncd): add durable local persistence"
```

### Task 7: Implement validated backup/export and restore

**Files:**
- Create: `caresyncd-web/src/persistence/backup.js`
- Create: `caresyncd-web/tests/persistence/backup.test.mjs`

**Interfaces:**
- Produces: `exportBackup(adapter) -> Promise<string>` and `importBackup(adapter, jsonText) -> Promise<{ profiles, runs }>`.

- [ ] **Step 1: Write tests for valid and invalid imports**

Test a successful round trip, malformed JSON, missing schemaVersion, unsupported future schema, and a corrupt run. Assert existing adapter data is unchanged after every rejected import.

- [ ] **Step 2: Run and verify failure**

Run: `node --test tests/persistence/backup.test.mjs`
Expected: FAIL.

- [ ] **Step 3: Implement two-phase import**

Parse and fully validate into memory first. Only after every record validates should the adapter be mutated. Before replacement, export the current state into `meta.previousKnownGoodBackup`.

- [ ] **Step 4: Run tests**

Run: `npm test`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add caresyncd-web/src/persistence/backup.js caresyncd-web/tests/persistence/backup.test.mjs
git commit -m "feat(caresyncd): add safe progress backup and restore"
```

### Task 8: Implement SessionController with autosave/resume

**Files:**
- Create: `caresyncd-web/src/app/session-controller.js`
- Create: `caresyncd-web/tests/app/session-controller.test.mjs`

**Interfaces:**
- Consumes: persistence adapter, engines, EventBus.
- Produces: `new SessionController({ persistence, eventBus })`, `startScenario(options)`, `startShift(options)`, `resumeActive()`, `dispatch(command)`, `completeActive()`, `getActiveSnapshot()`.

- [ ] **Step 1: Write failing lifecycle tests**

Test start→autosave, action→autosave, tick→autosave, close/recreate controller→resume, complete→move active record to completed runs, and persistence failure surfacing without clearing the in-memory session.

- [ ] **Step 2: Run and verify failure**

Run: `node --test tests/app/session-controller.test.mjs`
Expected: FAIL.

- [ ] **Step 3: Implement controller**

Every meaningful mutation calls `persistActive()` after engine state changes. Resume reconstructs engines from saved snapshots but never advances simulated time based on wall-clock downtime.

- [ ] **Step 4: Run the full core suite**

Run: `npm test`
Expected: all core tests PASS, including 8 mission catalog, 3 shift catalog, persistence, backup, resume, and no duplicate events.

- [ ] **Step 5: Commit**

```bash
git add caresyncd-web/src/app/session-controller.js caresyncd-web/tests/app/session-controller.test.mjs
git commit -m "feat(caresyncd): add autosaving session controller"
```

### Task 9: Add project integrity check and core acceptance gate

**Files:**
- Create: `caresyncd-web/scripts/check-project.mjs`
- Create: `caresyncd-web/tests/acceptance/core-parity.test.mjs`

**Interfaces:**
- Produces executable checks only; no runtime API.

- [ ] **Step 1: Write acceptance tests**

Create one scenario acceptance path and one `hospital_day` path that exercise start, actions, tick, dynamic admission, task delegation, handoff, save, reload, and debrief score generation.

- [ ] **Step 2: Implement `check-project.mjs`**

Check required files, exactly 8 scenarios, exactly 3 shifts, every definition version present, every shift scenario reference valid, and no source import begins with `/`.

- [ ] **Step 3: Run final Phase 1 verification**

Run:

```bash
npm test
npm run check
```

Expected: zero failures.

- [ ] **Step 4: Commit**

```bash
git add caresyncd-web/scripts caresyncd-web/tests/acceptance
git commit -m "test(caresyncd): lock browser core parity"
```

## Phase 1 Exit Criteria

Do not start the UI/PWA plan until all of these are true:

- `npm test` passes with zero failures.
- `npm run check` passes.
- 8 missions and 3 shifts are present and versioned.
- v3 clinical/shift acceptance paths pass.
- active runs survive controller recreation without wall-clock time drift.
- corrupt backup import cannot overwrite valid data.
- duplicate scheduled events cannot fire across resume boundaries.
- no module requires Node-only runtime APIs in `src/`.

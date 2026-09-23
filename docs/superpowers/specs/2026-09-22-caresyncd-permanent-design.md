# CareSyncD Permanent — Design Specification

**Date:** 2026-09-22  
**Branch:** `caresyncd-pages`  
**Status:** Approved design, implementation not yet started  
**Product:** CareSyncD — Clinical Judgment Under Pressure

## 1. Purpose

Create a permanent, free-to-host CareSyncD web application that does not depend on Jason's PC, Railway, Vercel Functions, a Node server, or a temporary tunnel. The application must preserve the CareSyncD v3 learner experience while moving simulation execution and persistence into the browser so it can be hosted as a static HTTPS site through GitHub Pages.

The server-based CareSyncD v3 release remains preserved separately as the server edition. The permanent edition is a browser-native deployment tier, not a replacement for the archived server build.

## 2. Success criteria

The permanent edition is successful when:

1. CareSyncD opens from a permanent HTTPS URL without any home computer running.
2. All 8 clinical missions remain playable.
3. All 3 hospital-shift simulations remain playable.
4. Multi-patient deterioration, admissions, tasks, interruptions, staffing changes, delegation, scoring, handoffs, and delayed consequences continue to work.
5. Learner progress survives browser refresh, app close, and device restart.
6. An interrupted shift can be resumed from the last autosaved state.
7. The app can be installed as a PWA and continue core simulation offline after first load.
8. The permanent deployment requires no paid runtime service.
9. The browser architecture can later add cloud accounts and classroom synchronization without rewriting the simulation engine.
10. CareSyncD remains clearly presented as educational simulation software and not a real-patient medical system.

## 3. Product boundary and safety

CareSyncD Permanent is an educational clinical simulation and workforce-training platform.

It must display and preserve the statement:

> Educational simulation only. Not for real-patient diagnosis, monitoring, or treatment.

The application must not claim:

- FDA clearance or medical-device status
- Accreditation or CE/CME approval
- Clinical efficacy or patient-outcome validation
- Real EHR integration
- Real patient monitoring
- Production-grade PHI storage

Learners should be told to use simulated information only and never enter PHI or real patient identifiers.

## 4. Architecture decision

### 4.1 Chosen architecture

Use a static browser-native PWA hosted by GitHub Pages.

The browser owns:

- simulation state
- shift clock
- patient deterioration
- admissions and interruptions
- task timing
- scoring
- achievements
- learner progress
- autosave/recovery
- debrief generation

No backend is required for the initial permanent edition.

### 4.2 Why this architecture

This architecture satisfies the immediate constraints:

- free hosting
- no PC dependency
- no Railway/Vercel resource requirement
- permanent HTTPS origin
- offline capability
- installable app experience
- future cloud migration path

### 4.3 Deferred capability

Real-time instructor-to-student synchronization across separate devices is intentionally deferred. Instructor controls remain available for the locally running simulation. Cross-device classroom orchestration becomes a later cloud feature.

## 5. Existing v3 capabilities that must survive

The permanent edition must preserve:

- 8 clinical missions
- 3 hospital-shift simulations
- Guided, Standard, and Challenge modes
- simultaneous multi-patient evolution
- time-dependent deterioration
- surprise events
- dynamic admissions
- call lights and monitor alarms
- family questions
- provider callbacks
- new orders
- medication/reassessment windows
- staffing changes
- delegation mechanics
- task priorities and due times
- overdue and missed-task consequences
- clinical actions satisfying related operational tasks
- discharge/transfer planning
- SBAR handoff
- end-of-shift handoff board
- clinical score and operations score
- competency domains
- achievements
- XP and rank progression
- full debrief and event timeline
- instructor local pause/resume/end controls

The goal is functional parity with v3's learner experience, not a stripped-down demo.

## 6. Application structure

Primary navigation:

- Home
- Clinical Missions
- Hospital Shifts
- Progress
- Debriefs
- Instructor
- Settings

Desktop uses a left sidebar. Mobile uses bottom navigation plus a More menu.

## 7. Home dashboard

Home should immediately expose:

- Resume Shift when an unfinished simulation exists
- Start a Hospital Shift
- Start a Clinical Mission
- current rank
- XP progress
- recent score
- last completed simulation
- achievement count
- competency snapshot
- installation/offline status

The primary learner action should be obvious within a few seconds.

## 8. Simulation cockpit

The cockpit has four conceptual zones.

### 8.1 Patient strip

Shows:

- room
- patient name
- priority
- status
- alerts
- state-change indicators

Learners can switch patients quickly.

### 8.2 Clinical workspace

Shows:

- vitals
- telemetry
- assessment findings
- labs
- imaging
- medications/orders
- chart
- notes

### 8.3 Action center

Groups actions into:

- Assessment
- Intervention
- Emergency
- Communication
- Reassessment
- Delegation

### 8.4 Shift operations

Shows:

- shift clock
- task queue
- overdue tasks
- call lights
- staffing
- admissions
- pending handoffs

On mobile, these zones become touch-friendly panels rather than a compressed desktop dashboard.

## 9. Visual urgency model

Urgency must be clear without creating visual chaos.

Use these semantic states:

- Stable — calm/default presentation
- Concerning — subtle warning
- Urgent — stronger visual emphasis
- Critical — unmistakable priority state
- Overdue — explicit task marker
- Newly changed — temporary highlight

Avoid unnecessary flashing or animation that competes with clinical decision-making.

## 10. Debrief design

Every completed simulation should produce a structured debrief with:

### What happened

A chronological shift/scenario timeline.

### What the learner did well

Specific decisions tied to scoring evidence.

### What could improve

Sequencing, prioritization, communication, safety, reassessment, and missed opportunities.

### Patient outcomes

Final simulated patient state for each patient.

### Operational performance

- missed tasks
- late tasks
- delegation decisions
- interruptions handled
- handoff completion

### Competency scores

- Assessment
- Intervention
- Safety
- Communication
- Reassessment
- Prioritization
- Shift Management

Actions at the end:

- Retry Shift
- Try Harder Difficulty
- Next Mission

## 11. Installable PWA behavior

The permanent edition should provide:

- web app manifest
- CareSyncD app icon
- standalone display mode
- splash/app-launch presentation
- service worker
- offline shell caching
- cached scenario/shift definitions
- offline simulation execution
- update-available notification
- deferred update activation until an active simulation is finished
- Android-friendly touch targets
- responsive tablet layout
- keyboard accessibility on desktop
- scalable text and accessible contrast

## 12. Core runtime modules

The browser implementation should separate concerns so future cloud features do not require rewriting the simulator.

### 12.1 SimulationEngine

Responsibilities:

- owns single-patient scenario state
- advances simulation time
- processes clinical actions
- applies deterioration and stabilization rules
- emits state transitions

### 12.2 ShiftEngine

Responsibilities:

- owns multi-patient assignment state
- evolves all active patients
- schedules admissions
- schedules operational events
- manages staffing
- manages delegation
- tracks timed tasks
- opens handoff windows
- calculates shift-level operational state

### 12.3 EventBus

Responsibilities:

- publishes state changes
- decouples UI from simulation logic
- provides a future synchronization seam for cloud classrooms

### 12.4 ScoringEngine

Responsibilities:

- clinical scoring
- operations scoring
- sequencing logic
- safety penalties
- competency domains
- achievements
- final grade/debrief inputs

### 12.5 PersistenceAdapter

The rest of the app must not access IndexedDB directly.

Initial implementation:

`PersistenceAdapter -> IndexedDB`

Future implementation:

`PersistenceAdapter -> authenticated cloud API`

This abstraction is mandatory for future-proofing.

## 13. Data model

### 13.1 ScenarioDefinition

Contains:

- scenarioId
- scenarioVersion
- title
- level
- starting patient state
- expected actions
- critical actions
- sequencing rules
- time-based events
- deterioration rules
- stabilization rules
- scoring rules
- debrief guidance

### 13.2 ShiftDefinition

Contains:

- shiftId
- shiftVersion
- title
- starting assignments
- scheduled admissions
- staffing model
- timed tasks
- interruptions
- order events
- escalation events
- handoff window
- operational scoring rules

### 13.3 LearnerProfile

Contains:

- learnerId
- displayName
- XP
- rank
- achievements
- preferences
- competency history
- createdAt
- updatedAt

### 13.4 SimulationRun

Contains:

- runId
- kind: scenario | shift
- definitionId
- definitionVersion
- difficulty
- startTime
- endTime
- status
- elapsedSimulationTime
- event timeline
- actions taken
- notes
- task outcomes
- patient outcomes
- competency scores
- final score
- achievements
- debrief

Every record includes `schemaVersion`.

## 14. Persistence strategy

Use IndexedDB for durable application data.

Use localStorage only for lightweight settings such as:

- theme
- sound preference
- accessibility preference
- last selected learner

IndexedDB stores:

- learner profiles
- active sessions
- completed runs
- debriefs
- achievements
- competency history
- XP/rank state
- local instructor settings
- backup metadata

## 15. Autosave and recovery

Autosave after every meaningful state mutation, including:

- clinical action
- patient switch
- task completion
- delegation
- new admission
- timed event
- new order
- learner note
- instructor control action
- score-affecting consequence

The home screen detects unfinished runs and presents:

**Shift in Progress — Resume**

Recovery restores the simulation clock and state from the last committed snapshot rather than restarting the shift.

## 16. Backup and restore

Provide:

- Export My Progress
- Import Progress

Export format: versioned JSON bundle.

Import behavior:

1. validate schema
2. validate required fields
3. reject malformed or unsupported data safely
4. preserve existing data until validation succeeds
5. create a local backup before replacing data

The app should keep a previous known-good snapshot for recovery from local corruption.

## 17. Versioning strategy

Scenario and shift definitions are independently versioned.

Historical results always retain the version used during the run.

Example:

A learner who completes `sepsis` definition version `3.1` retains that exact historical reference even if a later `4.0` definition becomes current.

Storage schemas also use explicit versions and migration functions.

## 18. Offline behavior

After first successful load, core CareSyncD should work offline.

Offline-capable features include:

- launching installed/cached missions
- launching installed/cached shifts
- simulation engine
- timers
- local instructor controls
- autosave
- resume
- scoring
- achievements
- debrief
- progress history

Cloud-only features introduced later must degrade gracefully when offline.

## 19. Update strategy

The service worker detects a new build but does not replace application code during an active simulation.

User experience:

**CareSyncD update available**  
**Install after this simulation**

When no active run exists, the learner may activate the update immediately.

## 20. Future cloud architecture

CareSyncD Permanent is designed as the first deployment tier of a larger system.

Future architecture:

`CareSyncD PWA -> Authenticated API -> Database -> Organization/Tenant Layer -> Cohorts -> Assignments -> Analytics -> Audit Trail -> LMS Integrations`

Future learner features may include:

- accounts
- cross-device progress
- cloud backup
- institutional assignments
- instructor feedback
- cohort membership
- authorized completion records

## 21. Future classroom mode

Future cloud Classroom Mode may support:

- instructor creates assignment
- learner joins by class code
- instructor sees active learners
- live patient state synchronization
- instructor event injection
- instructor observation of task completion
- debrief return to instructor dashboard
- cohort competency analytics

This is deliberately kept outside the local simulation engine so the offline product remains independent and stable.

## 22. Future institutional integrations

Potential future integrations include:

- LTI
- SCORM/xAPI
- institutional SSO
- LMS grade passback
- roster synchronization
- secure analytics export

These are future capabilities only and must not be represented as currently implemented.

## 23. Hosting and repository strategy

The permanent edition should be developed on the isolated `caresyncd-pages` branch so PeopleSyncD `main` remains untouched.

The existing CareSyncD v3 server release remains preserved separately.

The final static output must be GitHub Pages compatible and must not assume root-domain hosting. All asset paths and routing must work when served from a repository subpath such as:

`https://<owner>.github.io/<repository>/`

Client-side navigation must therefore avoid server-required rewrite rules. Prefer hash routing or static-page-safe navigation unless a Pages-compatible fallback strategy is explicitly implemented and tested.

## 24. Testing strategy

Implementation must include automated tests for:

- all 8 mission definitions load
- all 3 shift definitions load
- single-patient deterioration
- stabilization actions
- sequencing consequences
- surprise events
- multi-patient evolution
- admissions
- staffing changes
- delegation eligibility
- overdue task consequences
- linked task completion
- handoff generation
- scoring
- achievements
- persistence save/load
- active-session resume
- backup export/import validation
- schema migration
- offline-ready asset manifest integrity

Browser-level smoke tests should verify:

- first load
- start mission
- start shift
- save/resume
- complete/debrief
- installability metadata
- service worker registration
- offline reload after initial cache
- GitHub Pages repository-subpath asset loading

## 25. Non-goals for this implementation

Do not add the following during the permanent conversion unless separately designed and approved:

- real authentication backend
- real-time multi-device classroom synchronization
- billing
- EHR connectivity
- PHI storage
- institution tenancy
- LMS grade passback
- accreditation workflows
- medical-device functionality

## 26. Implementation principle

The permanent conversion is an infrastructure and architecture change, not permission to simplify the learner experience.

Where possible, port the tested v3 simulation rules rather than rewriting clinical behavior from scratch. Preserve scenario identifiers, scoring semantics, event behavior, and debrief meaning unless a specific incompatibility requires a documented change.

## 27. Final architecture summary

### CareSyncD Permanent

`GitHub Pages -> PWA UI -> EventBus -> SimulationEngine / ShiftEngine -> ScoringEngine -> PersistenceAdapter -> IndexedDB`

### Future CareSyncD Cloud

`Same PWA -> Same Engines -> Cloud Persistence/Sync Adapter -> Auth/API/Database/Classroom/Institution Services`

This boundary ensures the free permanent edition is useful on its own while remaining compatible with the eventual CareSyncD platform.

# PeopleSyncD HR Demo Readiness Design

**Date:** 2026-08-20  
**Status:** Approved design baseline  
**Target:** Demo-ready during the week of 2026-08-24, with functional cutoff by 2026-08-27 and validation/polish on 2026-08-28  
**Branch:** `demo/hr-mvp-readiness`

## 1. Objective

Deliver a convincing, real, persistent PeopleSyncD HR demonstration using the existing .NET 9, PostgreSQL, Next.js, tenant, authorization, MFA, passkey, audit, and deployment foundations.

The demo must prove one complete HR lifecycle rather than present disconnected screens or mock data-only prototypes.

Primary demo path:

`Sign in -> Executive Dashboard -> People Directory -> Add Employee -> Employee Profile -> Onboarding -> Credentials/Training -> Documents -> HR Case -> Employment Status -> Audit Timeline -> Management Report`

## 2. Scope Principles

1. Preserve the existing security and tenant architecture.
2. Build vertically through Domain, Application, Infrastructure, API, Web, tests, and demo seed data.
3. Use persisted PostgreSQL-backed records for all core HR demo workflows.
4. Prefer a smaller complete workflow over broad unfinished feature coverage.
5. Use realistic synthetic demo data only; no real employee, regulated, confidential, or production data.
6. Do not weaken MFA, WebAuthn, authorization, tenant isolation, session assurance, or audit controls to accelerate the demo.
7. Keep the design compatible with the existing Master Implementation Plan and future commercial MVP work.

## 3. Demo Personas

### Founder / HR Administrator
Can view workforce metrics, create and update employees, manage onboarding, credentials, document metadata, HR cases, employment status, and reports.

### Manager
Can view permitted workers in the selected organization and inspect appropriate employee and onboarding information. Restricted HR cases remain limited by authorization.

### Worker
Worker self-service is not required for the first demo. The domain and API should avoid assumptions that prevent a future worker workspace.

## 4. Functional Modules

### 4.1 Executive Dashboard

Display persisted organization-level summary cards and actionable lists:

- total employees;
- active employees;
- onboarding employees;
- employees on leave;
- credentials expiring soon;
- overdue onboarding tasks;
- open HR cases;
- recently changed employee records.

The dashboard must derive from the same underlying records used by the detailed screens.

### 4.2 People Directory

Provide a searchable organization-scoped employee list showing:

- display name;
- work email;
- employee number;
- title;
- department;
- manager;
- location;
- employment status;
- start date.

Search should support at minimum name, email, title, and department. Status filtering is required for the demo.

### 4.3 Employee Profile

Expand the existing `Employee` aggregate to support the demo lifecycle without introducing unnecessary enterprise complexity.

Required fields:

- organization ID;
- employee ID;
- employee number;
- display name;
- work email;
- title;
- department;
- manager employee ID, optional;
- location;
- employment type;
- employment status;
- start date;
- separation date, optional;
- created/updated audit timestamps where consistent with existing persistence patterns.

Allowed demo statuses:

- Onboarding
- Active
- Leave
- Suspended
- Separated
- Archived

Status transitions must be explicit and validated. A separated or archived employee must not silently become active through a generic edit operation.

### 4.4 Onboarding Center

Support versioned onboarding templates and employee onboarding instances.

For the demo, one default template is sufficient, containing representative tasks such as:

- employment paperwork;
- orientation;
- policy acknowledgement;
- required credentials;
- required training;
- equipment/access;
- manager introduction.

Each employee onboarding task must support:

- title;
- category;
- due date;
- status;
- completion timestamp;
- optional note.

Required statuses: NotStarted, InProgress, Completed, Waived.

The employee profile and dashboard must surface onboarding progress.

### 4.5 Credentials and Training

Track workforce credentials and training records.

Required fields:

- employee ID;
- type/category;
- name;
- issuer/provider;
- issued date, optional;
- expiration date, optional;
- status;
- reference/credential number, optional;
- note, optional.

The API and dashboard must calculate expiring-soon and expired states from persisted dates rather than storing only a presentation flag.

Demo warning window: 30 days.

### 4.6 Document Center

For the demo, implement secure document metadata and lifecycle without attempting a complete enterprise document-management system.

Required metadata:

- employee ID;
- category;
- display name;
- confidentiality level;
- uploaded/recorded timestamp;
- recorded by user;
- optional external/storage reference;
- status.

Supported categories should include at minimum Employment, Credential, Training, Policy, and Other.

Binary upload storage may be deferred if the repository does not already contain a production-ready protected object-storage pattern. The UI must clearly label metadata-only demo records when no binary is stored.

### 4.7 Restricted HR Cases

Provide a minimal but real restricted HR case workflow.

Required fields:

- case ID;
- employee ID;
- category;
- title;
- priority;
- status;
- assigned owner user ID;
- summary;
- created/updated timestamps;
- resolution note, optional.

Required statuses: Open, Investigating, Pending, Resolved, Closed.

Case access must use dedicated permissions and must not be implied merely by general employee-read permission.

### 4.8 Employment Lifecycle

Provide explicit actions for moving employees through the supported lifecycle.

At minimum:

- complete onboarding -> Active;
- place on leave;
- return from leave;
- suspend;
- separate with separation date;
- archive after separation.

Every lifecycle change must generate auditable evidence using the repository's existing audit/security event patterns where appropriate.

### 4.9 Audit Timeline

Show important employee-related events in chronological order, including:

- employee created;
- profile changed;
- onboarding task changed/completed;
- credential added/updated;
- document metadata added/updated;
- HR case created/status changed;
- employment status changed.

The demo timeline must be backed by persisted audit/event data, not client-only history.

### 4.10 Management Reporting

Provide at least two useful demo reports:

1. Workforce Summary: counts by status, department, and location.
2. Credential Risk: expired and expiring-within-30-days credentials.

CSV export is optional for this demo unless straightforward within existing patterns. On-screen report accuracy is required.

## 5. Data Model Boundaries

The demo should introduce or extend focused aggregates/entities rather than a single oversized employee record.

Recommended boundaries:

- `Employee` — core employment identity and lifecycle;
- `OnboardingTemplate` / `EmployeeOnboarding` — template and instantiated task state;
- `EmployeeCredential` — credential/training record;
- `EmployeeDocumentRecord` — document metadata;
- `HrCase` — restricted case workflow;
- audit events/records — use existing infrastructure and patterns where possible.

All records must be explicitly organization scoped, directly or through a guaranteed employee-to-organization relationship enforced by application and persistence boundaries.

## 6. API Design

Add versioned tenant-aware HR endpoints consistent with the existing REST/OpenAPI structure.

Minimum endpoint families:

- `/api/v1/employees`
- `/api/v1/employees/{employeeId}`
- `/api/v1/employees/{employeeId}/status`
- `/api/v1/employees/{employeeId}/onboarding`
- `/api/v1/employees/{employeeId}/credentials`
- `/api/v1/employees/{employeeId}/documents`
- `/api/v1/hr-cases`
- `/api/v1/hr-cases/{caseId}`
- `/api/v1/hr/dashboard`
- `/api/v1/hr/reports/workforce-summary`
- `/api/v1/hr/reports/credential-risk`

All tenant-bound routes must reject cross-tenant access even when a caller possesses a valid token for another organization.

## 7. Authorization

Reuse the existing permissions architecture and add narrowly scoped HR permissions where absent.

Suggested permission boundaries:

- `employees.read`
- `employees.write`
- `onboarding.read`
- `onboarding.write`
- `credentials.read`
- `credentials.write`
- `documents.read`
- `documents.write`
- `hr_cases.read`
- `hr_cases.write`
- `hr_reports.read`

Existing Owner/Administrator roles may receive these demo permissions through the established permission catalog. HR-case permissions remain separate from general employee permissions.

Privileged operations should continue to honor current authentication freshness/step-up policies where the existing system already requires them.

## 8. Web Experience

Add a coherent authenticated HR workspace within the existing Next.js application.

Primary navigation:

- Dashboard
- People
- Onboarding
- Credentials
- HR Cases
- Reports
- Security / Account

The employee profile should use tabs or clearly separated sections for Overview, Onboarding, Credentials, Documents, Cases, and Activity.

The design should optimize for a live presentation: meaningful empty states, clear badges, visible status, consistent actions, and fast navigation. It must remain functional without presentation-only hardcoded state.

## 9. Demo Seed Data

Add a deterministic synthetic demo organization dataset suitable for screenshots and live walkthroughs.

Recommended composition:

- 12-20 synthetic workers;
- several departments and locations;
- at least 2 managers;
- employees distributed across Active, Onboarding, Leave, and Separated states;
- at least 2 credentials expiring within 30 days;
- at least 1 expired credential;
- at least 2 onboarding employees with incomplete tasks;
- at least 2 open HR cases;
- enough completed and recent activity to populate the dashboard and timeline.

Seed data must be unmistakably fictional.

## 10. Error Handling

- Validation failures return structured problem details consistent with existing API conventions.
- Cross-tenant access returns the repository's established authorization/not-found behavior without disclosing foreign records.
- Invalid lifecycle transitions fail explicitly.
- Missing manager references, employee references, or case owners fail safely.
- Dashboard/report endpoints degrade by returning accurate empty aggregates rather than throwing when no records exist.
- Browser forms display actionable validation errors without exposing internal exception details.

## 11. Testing Strategy

The demo branch is not complete until the following are covered:

### Domain tests
- employee creation and validation;
- lifecycle transition rules;
- onboarding task state transitions;
- credential expiration calculations;
- HR case status transitions.

### Application/API tests
- create/read/update employee;
- organization isolation;
- permission enforcement;
- onboarding workflow;
- credentials workflow;
- HR case restricted authorization;
- dashboard aggregation;
- report aggregation.

### Persistence tests
- migrations apply to a clean PostgreSQL database;
- organization/employee relationships are enforced;
- required indexes/constraints exist;
- no cross-tenant query path is introduced accidentally.

### Web tests
- core demo navigation renders;
- employee creation/edit flow works;
- onboarding updates reflect in progress;
- credential warnings display correctly;
- HR case access honors authorization;
- dashboard uses live API data.

### Build/deployment gates
Existing repository quality gates must remain green. Demo readiness must not be claimed solely from local execution.

## 12. Demo Deployment

Use the existing Codespaces and/or persistent container deployment foundation rather than inventing a new hosting model.

For the demo, success requires one stable URL or launch procedure that can be exercised before presentation, backed by PostgreSQL and the real API/web stack.

The environment must use synthetic data and must be clearly labeled as a demo/non-production environment.

## 13. Explicitly Deferred

The following are out of scope for this demo increment unless already available with negligible integration effort:

- payroll processing;
- benefits administration;
- applicant tracking/recruiting;
- timekeeping/scheduling;
- billing/subscriptions;
- SAML implementation;
- SCIM implementation;
- advanced OIDC provider federation beyond current dependencies;
- enterprise e-signature;
- production-grade object storage redesign;
- complex workflow designer;
- mobile applications;
- broad Domonique 2.0 expansion;
- customer-specific integrations;
- production certification claims.

## 14. Demo Acceptance Criteria

The demo is ready when all of the following are true:

1. A Founder/HR user can authenticate using the existing PeopleSyncD identity stack.
2. The user can open a populated executive HR dashboard.
3. The user can create an employee and see the new employee persist after navigation/reload.
4. The user can update employee profile and employment status through validated actions.
5. The user can manage onboarding tasks and see progress reflected in the employee view and dashboard.
6. The user can add credentials/training and see expiration risk reflected in the dashboard/report.
7. The user can create and update restricted HR cases with permission enforcement.
8. The user can create/view employee document metadata.
9. The user can review an employee activity timeline backed by persisted evidence.
10. Workforce and credential-risk reports reflect current stored data.
11. Cross-tenant access tests pass.
12. Existing MFA/passkey/session security behavior remains intact.
13. Required repository CI gates pass on the demo branch.
14. A rehearsed demo environment can run the complete primary path without manual database editing or code changes.

## 15. Delivery Order

Implementation should proceed as vertical increments:

1. Employee core + persistence + API + directory/profile UI.
2. Onboarding workflow + dashboard metrics.
3. Credentials/training + expiration risk.
4. HR cases + restricted authorization.
5. Document metadata + audit timeline.
6. Reports + deterministic seed dataset.
7. Full demo rehearsal, CI remediation, deployment verification, and presentation polish.

Each increment must remain buildable and testable before starting the next.

# PeopleSyncD HR Demo Readiness Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver a persistent, tenant-safe PeopleSyncD HR demo that proves the complete path from authenticated HR dashboard through employee creation, onboarding, credentials, documents, HR cases, lifecycle changes, audit history, and management reporting.

**Architecture:** Extend the existing .NET 9 Clean Architecture modular monolith rather than creating a separate demo application. New HR aggregates live in `PeopleSyncD.Domain`, application services/repository contracts in `PeopleSyncD.Application`, PostgreSQL mappings and repositories in `PeopleSyncD.Infrastructure`, REST endpoints in `PeopleSyncD.Api`, and the presentation workflow in the existing Next.js 16 application. Every tenant-bound query accepts the authenticated tenant identifier and filters by it before returning records.

**Tech Stack:** .NET 9, ASP.NET Core, EF Core, PostgreSQL 16, xUnit, FluentValidation, Next.js 16.3.0, React 19.2.8, TypeScript 5.9.3, Docker Compose/Caddy, existing GitHub Actions and Codespaces deployment foundation.

**Spec:** `docs/superpowers/specs/2026-08-20-hr-demo-readiness-design.md`

## Global Constraints

- Preserve existing MFA, WebAuthn/passkey, session-assurance, permission, tenant-isolation, and audit behavior.
- Use persisted PostgreSQL-backed records for the core demo workflows; client-only state is not acceptable.
- Use synthetic demo data only; never seed real employee, confidential, regulated, or production data.
- Keep `hr_cases.read` / `hr_cases.write` separate from general employee permissions.
- Credential risk uses a 30-day warning window calculated from persisted expiration dates.
- Document binary storage is not required; metadata-only records must be visibly labeled when no protected storage reference exists.
- Allowed employee statuses: `Onboarding`, `Active`, `Leave`, `Suspended`, `Separated`, `Archived`.
- Existing repository CI gates must remain green before the branch is called demo-ready.

## File Structure Map

The implementation should keep one responsibility per file and follow the repository's existing controller/service/repository/configuration patterns.

**Domain**
- Modify `src/PeopleSyncD.Domain/Employees/Employee.cs` for employment profile and lifecycle behavior.
- Create `src/PeopleSyncD.Domain/Employees/EmploymentStatus.cs` and `EmploymentType.cs`.
- Create `src/PeopleSyncD.Domain/Onboarding/EmployeeOnboarding.cs`, `OnboardingTask.cs`, `OnboardingTaskStatus.cs`.
- Create `src/PeopleSyncD.Domain/Credentials/EmployeeCredential.cs`.
- Create `src/PeopleSyncD.Domain/Documents/EmployeeDocumentRecord.cs`.
- Create `src/PeopleSyncD.Domain/HrCases/HrCase.cs`, `HrCaseStatus.cs`, `HrCasePriority.cs`.
- Modify `src/PeopleSyncD.Domain/Permissions/Permission.cs` and `PermissionCatalog.cs`.

**Application**
- Create `src/PeopleSyncD.Application/Employees/EmployeeContracts.cs`, `EmployeeService.cs`, `EmployeeValidators.cs`.
- Create `src/PeopleSyncD.Application/Onboarding/OnboardingService.cs` and contracts.
- Create `src/PeopleSyncD.Application/Credentials/CredentialService.cs` and contracts.
- Create `src/PeopleSyncD.Application/Documents/DocumentRecordService.cs` and contracts.
- Create `src/PeopleSyncD.Application/HrCases/HrCaseService.cs` and contracts.
- Create `src/PeopleSyncD.Application/Hr/HrDashboardService.cs`, `HrReportingService.cs`, and contracts.
- Create repository contracts under `src/PeopleSyncD.Application/Interfaces/` for each new aggregate/query family.
- Modify `src/PeopleSyncD.Application/DependencyInjection.cs`.

**Infrastructure**
- Modify `src/PeopleSyncD.Infrastructure/Persistence/ApplicationDbContext.cs`.
- Modify `src/PeopleSyncD.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs`.
- Create focused EF configurations for onboarding, credentials, documents, and HR cases.
- Create repositories under `src/PeopleSyncD.Infrastructure/Repositories/`.
- Modify `src/PeopleSyncD.Infrastructure/DependencyInjection.cs`.
- Add one additive EF migration for the HR demo schema; never edit an already-applied historical migration.
- Add deterministic development/demo seeding through a new `DemoDataSeeder` called only in the approved demo/development path.

**API**
- Create `EmployeesController.cs`, `OnboardingController.cs`, `CredentialsController.cs`, `EmployeeDocumentsController.cs`, `HrCasesController.cs`, `HrDashboardController.cs`, and `HrReportsController.cs` under `src/PeopleSyncD.Api/Controllers/`.

**Web**
- Keep `src/PeopleSyncD.Web/lib/api.ts` as the transport primitive.
- Create `src/PeopleSyncD.Web/lib/hr-api.ts` for HR DTOs and calls.
- Create authenticated pages under `app/dashboard`, `app/people`, `app/people/[employeeId]`, `app/onboarding`, `app/credentials`, `app/hr-cases`, and `app/reports`.
- Modify `app/page.tsx`, `app/layout.tsx`, and `app/globals.css` for the demo shell/navigation without replacing current auth/security pages.

---

### Task 1: Employee Core Vertical Slice

**Files:**
- Modify: `src/PeopleSyncD.Domain/Employees/Employee.cs`
- Create: `src/PeopleSyncD.Domain/Employees/EmploymentStatus.cs`
- Create: `src/PeopleSyncD.Domain/Employees/EmploymentType.cs`
- Create: `src/PeopleSyncD.Application/Employees/EmployeeContracts.cs`
- Create: `src/PeopleSyncD.Application/Employees/EmployeeService.cs`
- Create: `src/PeopleSyncD.Application/Employees/EmployeeValidators.cs`
- Create: `src/PeopleSyncD.Application/Interfaces/IEmployeeRepository.cs`
- Modify: `src/PeopleSyncD.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs`
- Create: `src/PeopleSyncD.Infrastructure/Repositories/EmployeeRepository.cs`
- Modify: `src/PeopleSyncD.Infrastructure/DependencyInjection.cs`
- Modify: `src/PeopleSyncD.Application/DependencyInjection.cs`
- Create: `src/PeopleSyncD.Api/Controllers/EmployeesController.cs`
- Create: `tests/PeopleSyncD.Domain.Tests/EmployeeTests.cs`
- Create: `tests/PeopleSyncD.Api.Tests/EmployeeApiTests.cs`
- Create: `src/PeopleSyncD.Web/lib/hr-api.ts`
- Create: `src/PeopleSyncD.Web/app/people/page.tsx`
- Create: `src/PeopleSyncD.Web/app/people/[employeeId]/page.tsx`

**Interfaces:**
- Produces `EmployeeDto`, `CreateEmployeeRequest`, `UpdateEmployeeRequest`, `ChangeEmploymentStatusRequest`.
- Produces `IEmployeeRepository.GetAsync(Guid tenantId, Guid employeeId, CancellationToken)`, `ListAsync(Guid tenantId, string? search, EmploymentStatus? status, CancellationToken)`, `AddAsync(Employee employee, CancellationToken)`, `SaveChangesAsync(CancellationToken)`.

- [ ] **Step 1: Write failing domain tests for creation and lifecycle rules.**

```csharp
[Fact]
public void Create_defaults_to_onboarding_and_preserves_tenant()
{
    var tenantId = Guid.NewGuid();
    var result = Employee.Create(tenantId, "EFM-1001", "Jordan Carter", "jordan@example.test",
        "STEM Instructor", "Education", null, "St. Louis", EmploymentType.FullTime,
        new DateOnly(2026, 8, 24));

    Assert.True(result.IsSuccess);
    Assert.Equal(tenantId, result.Value.OrganizationId);
    Assert.Equal(EmploymentStatus.Onboarding, result.Value.Status);
}

[Fact]
public void Separated_employee_cannot_be_reactivated_by_generic_profile_update()
{
    var employee = CreateEmployee();
    Assert.True(employee.Activate().IsSuccess);
    Assert.True(employee.Separate(new DateOnly(2026, 8, 31)).IsSuccess);
    Assert.True(employee.UpdateProfile("Jordan Carter", "jordan@example.test", "Manager", "Education", null, "St. Louis", EmploymentType.FullTime).IsSuccess);
    Assert.Equal(EmploymentStatus.Separated, employee.Status);
}
```

- [ ] **Step 2: Run the domain test and verify failure.**

Run: `dotnet test tests/PeopleSyncD.Domain.Tests/PeopleSyncD.Domain.Tests.csproj --filter EmployeeTests`
Expected: FAIL because the expanded employee model and status transitions do not exist.

- [ ] **Step 3: Implement the employee aggregate and explicit lifecycle methods.**

Required public methods:

```csharp
Result UpdateProfile(string displayName, string email, string title, string department,
    Guid? managerEmployeeId, string location, EmploymentType employmentType);
Result Activate();
Result PlaceOnLeave();
Result ReturnFromLeave();
Result Suspend();
Result Separate(DateOnly separationDate);
Result Archive();
```

Reject invalid transitions with stable domain-error codes such as `employee.invalid_transition`; never accept status as a free-form generic edit field.

- [ ] **Step 4: Implement tenant-filtered repository, service contracts, validators, API endpoints, and permission checks.**

Controller routes:

```text
GET    /api/v1/employees?search=&status=
POST   /api/v1/employees
GET    /api/v1/employees/{employeeId}
PUT    /api/v1/employees/{employeeId}
POST   /api/v1/employees/{employeeId}/status
```

Every action must read `User.TryGetTenantId(out var tenantId)` before repository access. `employees.read` protects reads and `employees.write` protects mutations.

- [ ] **Step 5: Write API tests proving persistence intent, permissions, and cross-tenant denial.**

At minimum assert: owner can create/list/read; member without `employees.write` cannot create; a token for tenant B cannot retrieve tenant A's employee ID.

Run: `dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter EmployeeApiTests`
Expected: PASS.

- [ ] **Step 6: Add the first HR web flow.**

`hr-api.ts` must expose typed `listEmployees`, `getEmployee`, `createEmployee`, `updateEmployee`, and `changeEmploymentStatus` functions using existing `apiRequest<T>()`. The People page must support name/email/title/department search and status filter; the profile page must show employment fields and explicit status actions.

Run:

```bash
cd src/PeopleSyncD.Web
npm ci
npm run typecheck
npm run build
```

Expected: both commands PASS.

- [ ] **Step 7: Commit.**

```bash
git add src tests
git commit -m "feat: add tenant-safe employee HR workflow"
```

---

### Task 2: Onboarding and Executive Dashboard

**Files:**
- Create: `src/PeopleSyncD.Domain/Onboarding/OnboardingTaskStatus.cs`
- Create: `src/PeopleSyncD.Domain/Onboarding/OnboardingTask.cs`
- Create: `src/PeopleSyncD.Domain/Onboarding/EmployeeOnboarding.cs`
- Create: `src/PeopleSyncD.Application/Interfaces/IOnboardingRepository.cs`
- Create: `src/PeopleSyncD.Application/Onboarding/OnboardingContracts.cs`
- Create: `src/PeopleSyncD.Application/Onboarding/OnboardingService.cs`
- Create: `src/PeopleSyncD.Application/Hr/HrDashboardService.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/Configurations/EmployeeOnboardingConfiguration.cs`
- Create: `src/PeopleSyncD.Infrastructure/Repositories/OnboardingRepository.cs`
- Create: `src/PeopleSyncD.Api/Controllers/OnboardingController.cs`
- Create: `src/PeopleSyncD.Api/Controllers/HrDashboardController.cs`
- Create: `tests/PeopleSyncD.Domain.Tests/OnboardingTests.cs`
- Create: `tests/PeopleSyncD.Api.Tests/OnboardingDashboardApiTests.cs`
- Create: `src/PeopleSyncD.Web/app/dashboard/page.tsx`
- Create: `src/PeopleSyncD.Web/app/onboarding/page.tsx`

**Interfaces:**
- `EmployeeOnboarding.CreateDefault(Guid organizationId, Guid employeeId, DateOnly startDate)` creates seven deterministic demo tasks.
- `HrDashboardDto` returns `TotalEmployees`, `ActiveEmployees`, `OnboardingEmployees`, `LeaveEmployees`, `CredentialsExpiringSoon`, `OverdueOnboardingTasks`, `OpenHrCases`, and recent activity.

- [ ] **Step 1: Write failing tests for task transitions and progress.**

```csharp
[Fact]
public void Completed_tasks_drive_progress_percentage()
{
    var onboarding = EmployeeOnboarding.CreateDefault(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 24));
    var first = onboarding.Tasks.First();
    Assert.True(onboarding.CompleteTask(first.Id, DateTimeOffset.UtcNow, "Done").IsSuccess);
    Assert.Equal(1, onboarding.CompletedTaskCount);
    Assert.True(onboarding.ProgressPercent > 0);
}
```

- [ ] **Step 2: Run and verify failure.**

Run: `dotnet test tests/PeopleSyncD.Domain.Tests/PeopleSyncD.Domain.Tests.csproj --filter OnboardingTests`
Expected: FAIL.

- [ ] **Step 3: Implement persisted onboarding and dashboard queries.**

Statuses are exactly `NotStarted`, `InProgress`, `Completed`, `Waived`. Overdue means due date is before the current UTC date and status is neither Completed nor Waived.

- [ ] **Step 4: Add API endpoints.**

```text
GET /api/v1/employees/{employeeId}/onboarding
PUT /api/v1/employees/{employeeId}/onboarding/tasks/{taskId}
GET /api/v1/hr/dashboard
```

Use `onboarding.read` / `onboarding.write` permissions introduced in Task 4's permission update if not yet present; if implementing sequentially, add those constants here and add Owner/Administrator/Manager grants now, then Task 4 adds case-specific permissions only.

- [ ] **Step 5: Add dashboard and onboarding web pages using live API data.**

Dashboard cards and lists must render zero-state values rather than throw when the organization has no HR rows.

- [ ] **Step 6: Run targeted and web gates, then commit.**

```bash
dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter OnboardingDashboardApiTests
cd src/PeopleSyncD.Web && npm run typecheck && npm run build
cd ../..
git add src tests
git commit -m "feat: add onboarding and HR dashboard"
```

---

### Task 3: Credentials, Training, and Expiration Risk

**Files:**
- Create: `src/PeopleSyncD.Domain/Credentials/EmployeeCredential.cs`
- Create: `src/PeopleSyncD.Application/Interfaces/ICredentialRepository.cs`
- Create: `src/PeopleSyncD.Application/Credentials/CredentialContracts.cs`
- Create: `src/PeopleSyncD.Application/Credentials/CredentialService.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/Configurations/EmployeeCredentialConfiguration.cs`
- Create: `src/PeopleSyncD.Infrastructure/Repositories/CredentialRepository.cs`
- Create: `src/PeopleSyncD.Api/Controllers/CredentialsController.cs`
- Create: `tests/PeopleSyncD.Domain.Tests/EmployeeCredentialTests.cs`
- Create: `tests/PeopleSyncD.Api.Tests/CredentialApiTests.cs`
- Create: `src/PeopleSyncD.Web/app/credentials/page.tsx`

**Interfaces:**
- `CredentialRisk EvaluateRisk(DateOnly today)` returns `Current`, `ExpiringSoon`, or `Expired` without persisting the computed risk flag.
- ExpiringSoon is `expirationDate >= today && expirationDate <= today.AddDays(30)`.

- [ ] **Step 1: Write failing expiration tests.**

```csharp
[Theory]
[InlineData(0, "ExpiringSoon")]
[InlineData(30, "ExpiringSoon")]
[InlineData(31, "Current")]
[InlineData(-1, "Expired")]
public void Expiration_risk_is_calculated_from_dates(int days, string expected)
{
    var credential = EmployeeCredential.CreateForTest(new DateOnly(2026, 8, 20).AddDays(days));
    Assert.Equal(expected, credential.EvaluateRisk(new DateOnly(2026, 8, 20)).ToString());
}
```

- [ ] **Step 2: Implement domain, persistence, tenant-safe CRUD, and endpoints.**

```text
GET  /api/v1/employees/{employeeId}/credentials
POST /api/v1/employees/{employeeId}/credentials
PUT  /api/v1/employees/{employeeId}/credentials/{credentialId}
```

Update dashboard aggregation to count ExpiringSoon credentials.

- [ ] **Step 3: Add credentials UI with badges for Expired and Expiring soon.**

The web must not recompute risk differently from the API; consume the API's derived `risk` field.

- [ ] **Step 4: Verify and commit.**

```bash
dotnet test tests/PeopleSyncD.Domain.Tests/PeopleSyncD.Domain.Tests.csproj --filter EmployeeCredentialTests
dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter CredentialApiTests
cd src/PeopleSyncD.Web && npm run typecheck && npm run build
cd ../..
git add src tests
git commit -m "feat: add credential and training risk tracking"
```

---

### Task 4: Restricted HR Cases and Permission Boundaries

**Files:**
- Modify: `src/PeopleSyncD.Domain/Permissions/Permission.cs`
- Modify: `src/PeopleSyncD.Domain/Permissions/PermissionCatalog.cs`
- Create: `src/PeopleSyncD.Domain/HrCases/HrCase.cs`
- Create: `src/PeopleSyncD.Domain/HrCases/HrCaseStatus.cs`
- Create: `src/PeopleSyncD.Domain/HrCases/HrCasePriority.cs`
- Create: `src/PeopleSyncD.Application/Interfaces/IHrCaseRepository.cs`
- Create: `src/PeopleSyncD.Application/HrCases/HrCaseContracts.cs`
- Create: `src/PeopleSyncD.Application/HrCases/HrCaseService.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/Configurations/HrCaseConfiguration.cs`
- Create: `src/PeopleSyncD.Infrastructure/Repositories/HrCaseRepository.cs`
- Create: `src/PeopleSyncD.Api/Controllers/HrCasesController.cs`
- Create: `tests/PeopleSyncD.Domain.Tests/HrCaseTests.cs`
- Create: `tests/PeopleSyncD.Api.Tests/HrCaseAuthorizationApiTests.cs`
- Create: `src/PeopleSyncD.Web/app/hr-cases/page.tsx`

**Interfaces:**
- Add constants: `onboarding.read`, `onboarding.write`, `credentials.read`, `credentials.write`, `documents.read`, `documents.write`, `hr_cases.read`, `hr_cases.write`, `hr_reports.read` if not already introduced sequentially.
- Owner/Administrator receive all demo HR permissions. Manager receives employee/onboarding/credential permissions but **not** HR-case write by default. Member and Auditor do not gain HR-case access merely from employee read access.

- [ ] **Step 1: Write permission and status-transition tests first.**

```csharp
[Fact]
public void Employee_read_does_not_imply_hr_case_read()
{
    Assert.Contains(PermissionNames.EmployeesRead, PermissionCatalog.ForRole(TenantRole.Member));
    Assert.DoesNotContain(PermissionNames.HrCasesRead, PermissionCatalog.ForRole(TenantRole.Member));
}
```

- [ ] **Step 2: Implement HR case workflow.**

Statuses: `Open -> Investigating -> Pending -> Resolved -> Closed`; allow return from Pending to Investigating; reject Closed to Open through the normal status endpoint. Store summary and optional resolution note; do not place highly sensitive narrative data into audit metadata.

- [ ] **Step 3: Add protected endpoints and cross-tenant tests.**

```text
GET  /api/v1/hr-cases
POST /api/v1/hr-cases
GET  /api/v1/hr-cases/{caseId}
PUT  /api/v1/hr-cases/{caseId}
POST /api/v1/hr-cases/{caseId}/status
```

- [ ] **Step 4: Add HR Cases page with permission-aware error state.**

A 403 must render a clear access message; do not hide authorization failures as generic browser errors.

- [ ] **Step 5: Run and commit.**

```bash
dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter HrCaseAuthorizationApiTests
dotnet test tests/PeopleSyncD.Domain.Tests/PeopleSyncD.Domain.Tests.csproj --filter HrCaseTests
git add src tests
git commit -m "feat: add restricted HR case workflow"
```

---

### Task 5: Document Metadata and Persisted Employee Activity

**Files:**
- Create: `src/PeopleSyncD.Domain/Documents/EmployeeDocumentRecord.cs`
- Create: `src/PeopleSyncD.Application/Interfaces/IEmployeeDocumentRepository.cs`
- Create: `src/PeopleSyncD.Application/Documents/DocumentContracts.cs`
- Create: `src/PeopleSyncD.Application/Documents/DocumentRecordService.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/Configurations/EmployeeDocumentRecordConfiguration.cs`
- Create: `src/PeopleSyncD.Infrastructure/Repositories/EmployeeDocumentRepository.cs`
- Create: `src/PeopleSyncD.Api/Controllers/EmployeeDocumentsController.cs`
- Create: `src/PeopleSyncD.Application/Hr/EmployeeActivityService.cs`
- Create: `src/PeopleSyncD.Api/Controllers/EmployeeActivityController.cs`
- Create: `tests/PeopleSyncD.Api.Tests/DocumentActivityApiTests.cs`

**Interfaces:**
- Categories: `Employment`, `Credential`, `Training`, `Policy`, `Other`.
- Confidentiality values: `Standard`, `Restricted`.
- Activity query reads existing `SecurityAuditRecord` data through a new read-only application abstraction rather than exposing the persistence type directly.

- [ ] **Step 1: Write failing API tests for tenant-safe document metadata and activity.**

Assert a document record persists category/display name/confidentiality/storage reference, and that a foreign tenant cannot list it. Assert employee creation/status/credential/onboarding/case actions appear in chronological activity results after their services call `IAuditRecorder.RecordAsync`.

- [ ] **Step 2: Implement document metadata CRUD.**

```text
GET  /api/v1/employees/{employeeId}/documents
POST /api/v1/employees/{employeeId}/documents
PUT  /api/v1/employees/{employeeId}/documents/{documentId}
GET  /api/v1/employees/{employeeId}/activity
```

If `storageReference` is null, return `hasBinary=false`; the web must display `Metadata record only`.

- [ ] **Step 3: Add Documents and Activity sections to `app/people/[employeeId]/page.tsx`.**

- [ ] **Step 4: Verify and commit.**

```bash
dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter DocumentActivityApiTests
cd src/PeopleSyncD.Web && npm run typecheck && npm run build
cd ../..
git add src tests
git commit -m "feat: add HR document metadata and activity timeline"
```

---

### Task 6: Reporting, Deterministic Demo Data, and HR Schema Migration

**Files:**
- Create: `src/PeopleSyncD.Application/Hr/HrReportingService.cs`
- Create: `src/PeopleSyncD.Application/Hr/HrReportingContracts.cs`
- Create: `src/PeopleSyncD.Api/Controllers/HrReportsController.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/DemoDataSeeder.cs`
- Modify: `src/PeopleSyncD.Infrastructure/Persistence/DatabaseInitializer.cs`
- Modify: `src/PeopleSyncD.Infrastructure/Persistence/ApplicationDbContext.cs`
- Create: new additive migration under `src/PeopleSyncD.Infrastructure/Migrations/`
- Create: `tests/PeopleSyncD.Integration.Tests/HrDemoSchemaTests.cs`
- Create: `tests/PeopleSyncD.Api.Tests/HrReportingApiTests.cs`
- Create: `src/PeopleSyncD.Web/app/reports/page.tsx`

**Interfaces:**
- `WorkforceSummaryDto` groups counts by status, department, and location.
- `CredentialRiskReportDto` lists expired and expiring-within-30-days credentials.
- Seeder is idempotent by a fixed synthetic organization marker/seed key and must not run in Production.

- [ ] **Step 1: Add DbSets/configurations and generate one additive migration.**

Run from repository root:

```bash
dotnet ef migrations add HrDemoReadiness --project src/PeopleSyncD.Infrastructure --startup-project src/PeopleSyncD.Api
```

Expected: one new timestamped migration plus updated model snapshot; no existing migration changes.

- [ ] **Step 2: Write clean-database integration assertions before accepting the migration.**

Test that tables/indexes/FKs exist for employees, onboarding, credentials, documents, and HR cases and that required organization/employee relationships reject invalid references.

Run: `dotnet test tests/PeopleSyncD.Integration.Tests/PeopleSyncD.Integration.Tests.csproj --filter HrDemoSchemaTests`
Expected: PASS against PostgreSQL test infrastructure.

- [ ] **Step 3: Implement deterministic synthetic seed data.**

Seed 16 fictional workers across at least 4 departments, 2 locations, 2 managers, and Active/Onboarding/Leave/Separated statuses; include 2 credentials expiring within 30 days, 1 expired credential, 2 incomplete onboarding records, 2 open HR cases, document metadata, and audit activity. Use `.test` email addresses and names clearly marked as synthetic in the seed source.

- [ ] **Step 4: Implement reports and live reports page.**

```text
GET /api/v1/hr/reports/workforce-summary
GET /api/v1/hr/reports/credential-risk
```

Protect both with `hr_reports.read`.

- [ ] **Step 5: Run reporting/integration/web checks and commit.**

```bash
dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter HrReportingApiTests
dotnet test tests/PeopleSyncD.Integration.Tests/PeopleSyncD.Integration.Tests.csproj --filter HrDemoSchemaTests
cd src/PeopleSyncD.Web && npm run typecheck && npm run build
cd ../..
git add src tests
git commit -m "feat: add HR reporting schema and demo dataset"
```

---

### Task 7: Demo Shell, End-to-End Rehearsal, and Release Evidence

**Files:**
- Modify: `src/PeopleSyncD.Web/app/page.tsx`
- Modify: `src/PeopleSyncD.Web/app/layout.tsx`
- Modify: `src/PeopleSyncD.Web/app/globals.css`
- Create: `docs/demo/PEOPLESYNCD-HR-DEMO-RUNBOOK.md`
- Create: `tests/acceptance/hr-demo-smoke.md` or the repository's accepted executable smoke-test equivalent if an established acceptance runner exists.

**Interfaces:**
- Primary navigation order: Dashboard, People, Onboarding, Credentials, HR Cases, Reports, Security/Account.
- Rehearsal path: sign in -> dashboard -> create employee -> profile -> onboarding task -> credential -> document metadata -> HR case -> status change -> activity -> reports.

- [ ] **Step 1: Replace the M2.1-centric home presentation with a demo launch shell without removing auth/security routes.**

The landing copy must identify the environment as a non-production PeopleSyncD HR demo and provide direct navigation to the authenticated HR workspace.

- [ ] **Step 2: Run all .NET and web quality gates locally.**

```bash
dotnet test PeopleSyncD.slnx --configuration Release
cd src/PeopleSyncD.Web
npm ci
npm run typecheck
npm run build
npm audit --audit-level=high
```

Expected: zero failing tests, successful typecheck/build, and no high-severity npm audit failure.

- [ ] **Step 3: Validate the container/deployment contract.**

Use the repository's existing Codespaces or persistent Compose path. Do not create another hosting architecture. Confirm API, web, gateway, and PostgreSQL start with the demo seed enabled only in the demo environment and that the app is reachable through one HTTPS origin.

- [ ] **Step 4: Execute the complete rehearsal twice from a clean browser session.**

Pass criteria: no database editing, no code changes, no broken navigation, no cross-tenant leakage, and the employee created during rehearsal remains present after reload.

- [ ] **Step 5: Write the runbook with exact launch, sign-in, reset/seed, rehearsal, and fallback steps.**

The runbook must explicitly say `Demo / Non-Production` and identify synthetic data expectations.

- [ ] **Step 6: Commit final demo polish and push for CI.**

```bash
git add src tests docs/demo
git commit -m "feat: finalize PeopleSyncD HR demo readiness"
git push origin demo/hr-mvp-readiness
```

- [ ] **Step 7: Do not declare demo-ready until GitHub Actions are green on the final head.**

Required evidence includes the existing .NET platform, API contracts, security, governance/traceability, Markdown, build/container, and applicable deployment workflows. If any required job fails, fix the cause on this branch and rerun; do not bypass or disable a quality gate to meet the presentation date.

## Final Acceptance Checklist

- [ ] Existing authentication, MFA/passkeys, tenant selection, and session security still pass.
- [ ] Executive HR dashboard is populated from real persisted rows.
- [ ] Employee create/edit/status operations persist and survive reload.
- [ ] Invalid employment lifecycle transitions are rejected.
- [ ] Onboarding progress updates dashboard and employee view.
- [ ] Credential expiration risk is date-derived and consistent in dashboard/report.
- [ ] HR cases require dedicated permissions and reject cross-tenant access.
- [ ] Document metadata persists and metadata-only records are clearly labeled.
- [ ] Employee activity is persisted through audit records.
- [ ] Workforce Summary and Credential Risk reports reflect current records.
- [ ] Demo seed is deterministic, fictional, idempotent, and disabled in Production.
- [ ] Clean PostgreSQL migration test passes.
- [ ] Next.js typecheck/build pass.
- [ ] Full `.NET` solution tests pass.
- [ ] Final required GitHub Actions are green.
- [ ] Complete demo path succeeds twice without manual intervention.

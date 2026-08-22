# PeopleSyncD HR Demo Readiness Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver a persistent, tenant-safe PeopleSyncD HR demo that proves the complete path from authenticated HR dashboard through employee creation, onboarding, credentials, documents, HR cases, lifecycle changes, audit history, and management reporting.

**Architecture:** Extend the existing .NET 9 Clean Architecture modular monolith. New HR aggregates live in `PeopleSyncD.Domain`; services and repository contracts in `PeopleSyncD.Application`; PostgreSQL mappings/repositories in `PeopleSyncD.Infrastructure`; REST endpoints in `PeopleSyncD.Api`; and the presentation workflow in the existing Next.js 16 application. Every tenant-bound repository method accepts `tenantId` and filters by it before returning a row.

**Tech Stack:** .NET 9, ASP.NET Core, EF Core, PostgreSQL 16, xUnit, FluentValidation, Next.js 16.3.0, React 19.2.8, TypeScript 5.9.3, Docker Compose/Caddy, existing GitHub Actions and Codespaces deployment foundation.

**Spec:** `docs/superpowers/specs/2026-08-20-hr-demo-readiness-design.md`

## Global Constraints

- Preserve existing MFA, WebAuthn/passkey, session-assurance, authorization, tenant-isolation, and audit behavior.
- Core demo workflows must persist to PostgreSQL; client-only state is not acceptable.
- Seed synthetic demo data only; never seed real employee, confidential, regulated, or production data.
- Keep `hr_cases.read` and `hr_cases.write` separate from general employee permissions.
- Credential risk is calculated from persisted dates using a 30-day warning window.
- Document binary storage is deferred; metadata-only records must return `hasBinary=false` and the UI must label them `Metadata record only`.
- Employee statuses are exactly `Onboarding`, `Active`, `Leave`, `Suspended`, `Separated`, `Archived`.
- Onboarding task statuses are exactly `NotStarted`, `InProgress`, `Completed`, `Waived`.
- HR case statuses are exactly `Open`, `Investigating`, `Pending`, `Resolved`, `Closed`.
- Do not edit historical migrations. Add one new additive migration for the HR demo schema.
- Existing required CI gates must remain green before the branch is called demo-ready.

## File Structure Map

### Domain

- Modify `src/PeopleSyncD.Domain/Employees/Employee.cs`.
- Create `src/PeopleSyncD.Domain/Employees/EmploymentStatus.cs`, `EmploymentType.cs`.
- Create `src/PeopleSyncD.Domain/Onboarding/OnboardingTemplate.cs`, `OnboardingTemplateTask.cs`, `EmployeeOnboarding.cs`, `OnboardingTask.cs`, `OnboardingTaskStatus.cs`.
- Create `src/PeopleSyncD.Domain/Credentials/EmployeeCredential.cs`, `CredentialRisk.cs`.
- Create `src/PeopleSyncD.Domain/Documents/EmployeeDocumentRecord.cs`.
- Create `src/PeopleSyncD.Domain/HrCases/HrCase.cs`, `HrCaseStatus.cs`, `HrCasePriority.cs`.
- Modify `src/PeopleSyncD.Domain/Permissions/Permission.cs`, `PermissionCatalog.cs`.

### Application

- Create focused folders/files under `Employees`, `Onboarding`, `Credentials`, `Documents`, `HrCases`, and `Hr`.
- Create repository contracts under `src/PeopleSyncD.Application/Interfaces/`.
- Modify `src/PeopleSyncD.Application/DependencyInjection.cs`.

### Infrastructure

- Modify `src/PeopleSyncD.Infrastructure/Persistence/ApplicationDbContext.cs` and `DatabaseInitializer.cs`.
- Modify `Persistence/Configurations/EmployeeConfiguration.cs`; create focused configurations for onboarding, credentials, documents, and HR cases.
- Create repositories under `src/PeopleSyncD.Infrastructure/Repositories/`.
- Create `src/PeopleSyncD.Infrastructure/Persistence/DemoDataSeeder.cs`.
- Modify `src/PeopleSyncD.Infrastructure/DependencyInjection.cs`.
- Add one EF migration under `src/PeopleSyncD.Infrastructure/Migrations/`.

### API

- Create controllers: `EmployeesController`, `OnboardingController`, `CredentialsController`, `EmployeeDocumentsController`, `EmployeeActivityController`, `HrCasesController`, `HrDashboardController`, `HrReportsController`.

### Web

- Keep `src/PeopleSyncD.Web/lib/api.ts` as the HTTP primitive.
- Create `src/PeopleSyncD.Web/lib/hr-api.ts`.
- Create pages under `app/dashboard`, `app/people`, `app/people/[employeeId]`, `app/onboarding`, `app/credentials`, `app/hr-cases`, `app/reports`.
- Modify `app/page.tsx`, `app/layout.tsx`, `app/globals.css` without removing current auth/security routes.

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
- Modify: `src/PeopleSyncD.Application/DependencyInjection.cs`
- Modify: `src/PeopleSyncD.Infrastructure/DependencyInjection.cs`
- Create: `src/PeopleSyncD.Api/Controllers/EmployeesController.cs`
- Create: `tests/PeopleSyncD.Domain.Tests/EmployeeTests.cs`
- Create: `tests/PeopleSyncD.Api.Tests/EmployeeApiTests.cs`
- Create: `src/PeopleSyncD.Web/lib/hr-api.ts`
- Create: `src/PeopleSyncD.Web/app/people/page.tsx`
- Create: `src/PeopleSyncD.Web/app/people/[employeeId]/page.tsx`

**Interfaces:**

```csharp
public sealed record CreateEmployeeRequest(
    string EmployeeNumber, string DisplayName, string WorkEmail, string Title,
    string Department, Guid? ManagerEmployeeId, string Location,
    EmploymentType EmploymentType, DateOnly StartDate);

public sealed record UpdateEmployeeRequest(
    string DisplayName, string WorkEmail, string Title, string Department,
    Guid? ManagerEmployeeId, string Location, EmploymentType EmploymentType);

public sealed record ChangeEmploymentStatusRequest(EmploymentStatus Status, DateOnly? EffectiveDate);
```

`IEmployeeRepository` must expose `GetAsync(Guid tenantId, Guid employeeId, CancellationToken)`, `ListAsync(Guid tenantId, string? search, EmploymentStatus? status, CancellationToken)`, `AddAsync(Employee employee, CancellationToken)`, and `SaveChangesAsync(CancellationToken)`.

- [ ] **Step 1: Write failing employee domain tests.**

```csharp
[Fact]
public void Create_defaults_to_onboarding_and_preserves_tenant()
{
    var tenantId = Guid.NewGuid();
    var result = Employee.Create(
        tenantId, "PSD-1001", "Jordan Carter", "jordan@example.test",
        "HR Specialist", "People Operations", null, "St. Louis",
        EmploymentType.FullTime, new DateOnly(2026, 8, 24));

    Assert.True(result.IsSuccess);
    Assert.Equal(tenantId, result.Value.OrganizationId);
    Assert.Equal(EmploymentStatus.Onboarding, result.Value.Status);
}

[Fact]
public void Separated_employee_stays_separated_after_profile_edit()
{
    var employee = TestEmployees.CreateOnboarding();
    Assert.True(employee.Activate().IsSuccess);
    Assert.True(employee.Separate(new DateOnly(2026, 8, 31)).IsSuccess);
    Assert.True(employee.UpdateProfile(
        "Jordan Carter", "jordan@example.test", "Manager", "People Operations",
        null, "St. Louis", EmploymentType.FullTime).IsSuccess);
    Assert.Equal(EmploymentStatus.Separated, employee.Status);
}
```

Create `tests/PeopleSyncD.Domain.Tests/TestEmployees.cs` with a concrete `CreateOnboarding()` factory that calls the public `Employee.Create(...)` signature above; do not add test-only methods to production entities.

- [ ] **Step 2: Run the test to verify failure.**

Run: `dotnet test tests/PeopleSyncD.Domain.Tests/PeopleSyncD.Domain.Tests.csproj --filter EmployeeTests`
Expected: FAIL because the expanded employee model does not exist.

- [ ] **Step 3: Implement the employee aggregate and explicit transitions.**

Required methods:

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

Rules: Onboarding may activate; Active may go to Leave/Suspended/Separated; Leave may return to Active or separate; Suspended may return to Active or separate; Separated may archive; Archived is terminal. Generic profile editing never changes status.

- [ ] **Step 4: Implement repository, service, validation, controller, and permission enforcement.**

Routes:

```text
GET    /api/v1/employees?search=&status=
POST   /api/v1/employees
GET    /api/v1/employees/{employeeId}
PUT    /api/v1/employees/{employeeId}
POST   /api/v1/employees/{employeeId}/status
```

All actions read `User.TryGetTenantId(out var tenantId)` before repository access. Reads require `employees.read`; mutations require `employees.write`. A manager reference is valid only if the manager exists in the same tenant.

- [ ] **Step 5: Write API tests for create/list/read/update/status and cross-tenant denial.**

Run: `dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter EmployeeApiTests`
Expected: PASS.

- [ ] **Step 6: Add typed web API functions and People directory/profile pages.**

`hr-api.ts` exports `listEmployees`, `getEmployee`, `createEmployee`, `updateEmployee`, `changeEmploymentStatus`. Directory supports search by name/email/title/department and status filter. Profile shows employee number, title, department, manager, location, type, start/separation dates, and explicit status actions.

Run:

```bash
cd src/PeopleSyncD.Web
npm ci
npm run typecheck
npm run build
```

Expected: PASS.

- [ ] **Step 7: Commit.**

```bash
git add src tests
git commit -m "feat: add tenant-safe employee HR workflow"
```

---

### Task 2: Versioned Onboarding and Executive Dashboard

**Files:**

- Create: `src/PeopleSyncD.Domain/Onboarding/OnboardingTemplate.cs`
- Create: `src/PeopleSyncD.Domain/Onboarding/OnboardingTemplateTask.cs`
- Create: `src/PeopleSyncD.Domain/Onboarding/EmployeeOnboarding.cs`
- Create: `src/PeopleSyncD.Domain/Onboarding/OnboardingTask.cs`
- Create: `src/PeopleSyncD.Domain/Onboarding/OnboardingTaskStatus.cs`
- Create: `src/PeopleSyncD.Application/Interfaces/IOnboardingRepository.cs`
- Create: `src/PeopleSyncD.Application/Onboarding/OnboardingContracts.cs`
- Create: `src/PeopleSyncD.Application/Onboarding/OnboardingService.cs`
- Create: `src/PeopleSyncD.Application/Hr/HrDashboardContracts.cs`
- Create: `src/PeopleSyncD.Application/Hr/HrDashboardService.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/Configurations/OnboardingTemplateConfiguration.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/Configurations/EmployeeOnboardingConfiguration.cs`
- Create: `src/PeopleSyncD.Infrastructure/Repositories/OnboardingRepository.cs`
- Create: `src/PeopleSyncD.Api/Controllers/OnboardingController.cs`
- Create: `src/PeopleSyncD.Api/Controllers/HrDashboardController.cs`
- Create: `tests/PeopleSyncD.Domain.Tests/OnboardingTests.cs`
- Create: `tests/PeopleSyncD.Api.Tests/OnboardingDashboardApiTests.cs`
- Create: `src/PeopleSyncD.Web/app/dashboard/page.tsx`
- Create: `src/PeopleSyncD.Web/app/onboarding/page.tsx`

**Interfaces:**

`OnboardingTemplate` has `OrganizationId`, `Name`, integer `Version`, `IsActive`, and ordered template tasks. Seed one active template named `Standard Employee Onboarding`, version `1`, containing Employment Paperwork, Orientation, Policy Acknowledgement, Required Credentials, Required Training, Equipment/Access, and Manager Introduction.

`EmployeeOnboarding.Instantiate(OnboardingTemplate template, Guid employeeId, DateOnly startDate)` copies the template version and task definitions into persisted employee task state. Existing employee onboarding instances do not mutate when a future template version is created.

- [ ] **Step 1: Write failing template/version/progress tests.**

```csharp
[Fact]
public void Instance_keeps_template_version_and_progress()
{
    var template = OnboardingTemplate.CreateStandard(Guid.NewGuid(), 1).Value;
    var onboarding = EmployeeOnboarding.Instantiate(template, Guid.NewGuid(), new DateOnly(2026, 8, 24)).Value;
    Assert.Equal(1, onboarding.TemplateVersion);
    Assert.Equal(7, onboarding.Tasks.Count);

    var first = onboarding.Tasks.First();
    Assert.True(onboarding.CompleteTask(first.Id, DateTimeOffset.UtcNow, "Done").IsSuccess);
    Assert.Equal(1, onboarding.CompletedTaskCount);
    Assert.True(onboarding.ProgressPercent > 0);
}
```

- [ ] **Step 2: Run and verify failure.**

Run: `dotnet test tests/PeopleSyncD.Domain.Tests/PeopleSyncD.Domain.Tests.csproj --filter OnboardingTests`
Expected: FAIL.

- [ ] **Step 3: Implement template persistence, employee instances, and dashboard query.**

Overdue means due date is before the current UTC date and status is neither Completed nor Waived. Dashboard DTO fields: total employees, active, onboarding, leave, credentials expiring soon, overdue onboarding tasks, open HR cases, recently changed employees.

- [ ] **Step 4: Add permissions and routes.**

Add `onboarding.read` and `onboarding.write` to `PermissionNames` and role catalog. Owner/Administrator/Manager get both; Member/Auditor get read only if existing role semantics permit employee operational visibility, otherwise no onboarding grant.

```text
GET /api/v1/employees/{employeeId}/onboarding
PUT /api/v1/employees/{employeeId}/onboarding/tasks/{taskId}
GET /api/v1/hr/dashboard
```

Completing all non-waived tasks makes the employee eligible for `Activate()` but does not silently change employment status; the explicit status action remains auditable.

- [ ] **Step 5: Add dashboard/onboarding pages and API tests.**

Run:

```bash
dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter OnboardingDashboardApiTests
cd src/PeopleSyncD.Web && npm run typecheck && npm run build
```

Expected: PASS.

- [ ] **Step 6: Commit.**

```bash
cd ../..
git add src tests
git commit -m "feat: add versioned onboarding and HR dashboard"
```

---

### Task 3: Credentials, Training, and Expiration Risk

**Files:**

- Create: `src/PeopleSyncD.Domain/Credentials/CredentialRisk.cs`
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

```csharp
public static Result<EmployeeCredential> Create(
    Guid organizationId, Guid employeeId, string category, string name,
    string issuer, DateOnly? issuedDate, DateOnly? expirationDate,
    string? referenceNumber, string? note);

public CredentialRisk EvaluateRisk(DateOnly today);
```

`EvaluateRisk`: no expiration date => Current; date before today => Expired; date from today through today+30 inclusive => ExpiringSoon; later => Current.

- [ ] **Step 1: Write failing risk tests using only public production APIs.**

```csharp
[Theory]
[InlineData(-1, CredentialRisk.Expired)]
[InlineData(0, CredentialRisk.ExpiringSoon)]
[InlineData(30, CredentialRisk.ExpiringSoon)]
[InlineData(31, CredentialRisk.Current)]
public void Risk_is_derived_from_expiration_date(int days, CredentialRisk expected)
{
    var today = new DateOnly(2026, 8, 20);
    var credential = EmployeeCredential.Create(
        Guid.NewGuid(), Guid.NewGuid(), "License", "Demo Credential",
        "Synthetic Board", today.AddYears(-1), today.AddDays(days), "DEMO-1", null).Value;
    Assert.Equal(expected, credential.EvaluateRisk(today));
}
```

- [ ] **Step 2: Implement domain, tenant-safe repository/service, permissions, and routes.**

Add `credentials.read` / `credentials.write` to permissions. Owner/Administrator/Manager get both; read-only roles receive only what current employee visibility policy supports.

```text
GET  /api/v1/employees/{employeeId}/credentials
POST /api/v1/employees/{employeeId}/credentials
PUT  /api/v1/employees/{employeeId}/credentials/{credentialId}
```

Dashboard `credentialsExpiringSoon` uses the same `EvaluateRisk` rule.

- [ ] **Step 3: Add credentials page with API-provided risk badges.**

The browser must not recalculate risk differently from the API.

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

Add `hr_cases.read`, `hr_cases.write`. Owner/Administrator receive both. Manager receives neither by default. Member/Auditor receive neither. Employee read never implies case access.

- [ ] **Step 1: Write failing permission and transition tests.**

```csharp
[Fact]
public void Employee_read_does_not_imply_hr_case_read()
{
    Assert.Contains(PermissionNames.EmployeesRead, PermissionCatalog.ForRole(TenantRole.Member));
    Assert.DoesNotContain(PermissionNames.HrCasesRead, PermissionCatalog.ForRole(TenantRole.Member));
}
```

Allowed normal transitions: Open->Investigating, Open->Pending, Investigating->Pending, Investigating->Resolved, Pending->Investigating, Pending->Resolved, Resolved->Closed. Closed is terminal.

- [ ] **Step 2: Implement HR case aggregate, tenant filtering, services, and routes.**

```text
GET  /api/v1/hr-cases
POST /api/v1/hr-cases
GET  /api/v1/hr-cases/{caseId}
PUT  /api/v1/hr-cases/{caseId}
POST /api/v1/hr-cases/{caseId}/status
```

Audit metadata may contain case ID/category/status but must not copy sensitive case narrative or resolution text.

- [ ] **Step 3: Prove dedicated authorization and cross-tenant denial in API tests.**

Run: `dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter HrCaseAuthorizationApiTests`
Expected: PASS.

- [ ] **Step 4: Add HR Cases page; render 403 as an explicit access message.**

- [ ] **Step 5: Commit.**

```bash
git add src tests
git commit -m "feat: add restricted HR case workflow"
```

---

### Task 5: Document Metadata and Persisted Employee Activity

**Files:**

- Create: `src/PeopleSyncD.Domain/Documents/EmployeeDocumentRecord.cs`
- Create: `src/PeopleSyncD.Application/Interfaces/IEmployeeDocumentRepository.cs`
- Create: `src/PeopleSyncD.Application/Interfaces/IEmployeeActivityReader.cs`
- Create: `src/PeopleSyncD.Application/Documents/DocumentContracts.cs`
- Create: `src/PeopleSyncD.Application/Documents/DocumentRecordService.cs`
- Create: `src/PeopleSyncD.Application/Hr/EmployeeActivityService.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/Configurations/EmployeeDocumentRecordConfiguration.cs`
- Create: `src/PeopleSyncD.Infrastructure/Repositories/EmployeeDocumentRepository.cs`
- Create: `src/PeopleSyncD.Infrastructure/Repositories/EmployeeActivityReader.cs`
- Create: `src/PeopleSyncD.Api/Controllers/EmployeeDocumentsController.cs`
- Create: `src/PeopleSyncD.Api/Controllers/EmployeeActivityController.cs`
- Create: `tests/PeopleSyncD.Api.Tests/DocumentActivityApiTests.cs`
- Modify: `src/PeopleSyncD.Web/app/people/[employeeId]/page.tsx`

**Interfaces:**

Document categories: `Employment`, `Credential`, `Training`, `Policy`, `Other`. Confidentiality: `Standard`, `Restricted`. Add `documents.read` / `documents.write`; Owner/Administrator get both, Manager gets read/write only if the demo's employee-management role is intended to maintain records, while Restricted document records still require the higher permission decision implemented in the service.

- [ ] **Step 1: Write failing API tests for metadata persistence, `hasBinary`, activity, and tenant isolation.**

- [ ] **Step 2: Implement metadata routes and activity reader.**

```text
GET  /api/v1/employees/{employeeId}/documents
POST /api/v1/employees/{employeeId}/documents
PUT  /api/v1/employees/{employeeId}/documents/{documentId}
GET  /api/v1/employees/{employeeId}/activity
```

`hasBinary` is `storageReference is not null`. Activity is read from existing persisted `SecurityAuditRecord` rows through `IEmployeeActivityReader`; do not expose the infrastructure type to the API.

- [ ] **Step 3: Ensure EmployeeService, OnboardingService, CredentialService, DocumentRecordService, and HrCaseService call `IAuditRecorder.RecordAsync` for demo-visible changes.**

Event types are stable strings: `employee.created`, `employee.profile_updated`, `employee.status_changed`, `onboarding.task_changed`, `credential.created`, `credential.updated`, `document.created`, `document.updated`, `hr_case.created`, `hr_case.status_changed`.

- [ ] **Step 4: Add Documents and Activity sections to employee profile.**

Metadata without binary shows `Metadata record only`.

- [ ] **Step 5: Verify and commit.**

```bash
dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter DocumentActivityApiTests
cd src/PeopleSyncD.Web && npm run typecheck && npm run build
cd ../..
git add src tests
git commit -m "feat: add HR document metadata and activity timeline"
```

---

### Task 6: Reports, Additive Migration, and Deterministic Demo Seed

**Files:**

- Modify: `src/PeopleSyncD.Infrastructure/Persistence/ApplicationDbContext.cs`
- Modify: `src/PeopleSyncD.Infrastructure/Persistence/DatabaseInitializer.cs`
- Create: `src/PeopleSyncD.Infrastructure/Persistence/DemoDataSeeder.cs`
- Create: additive migration in `src/PeopleSyncD.Infrastructure/Migrations/`
- Create: `src/PeopleSyncD.Application/Hr/HrReportingContracts.cs`
- Create: `src/PeopleSyncD.Application/Hr/HrReportingService.cs`
- Create: `src/PeopleSyncD.Api/Controllers/HrReportsController.cs`
- Create: `tests/PeopleSyncD.Integration.Tests/HrDemoSchemaTests.cs`
- Create: `tests/PeopleSyncD.Api.Tests/HrReportingApiTests.cs`
- Create: `src/PeopleSyncD.Web/app/reports/page.tsx`

**Interfaces:**

Add `hr_reports.read`; Owner/Administrator/Manager/Auditor receive it, Member does not by default. `WorkforceSummaryDto` groups by status, department, location. `CredentialRiskReportDto` lists expired and 30-day expiring credentials.

- [ ] **Step 1: Add DbSets/configurations and generate one additive migration.**

```bash
dotnet ef migrations add HrDemoReadiness --project src/PeopleSyncD.Infrastructure --startup-project src/PeopleSyncD.Api
```

Expected: one new migration plus model snapshot changes; historical migration files remain byte-for-byte unchanged.

- [ ] **Step 2: Write clean PostgreSQL schema tests.**

Assert tables/indexes/FKs for employees, onboarding templates/instances/tasks, credentials, document records, and HR cases. Assert invalid employee/organization relationships fail at persistence boundaries.

Run: `dotnet test tests/PeopleSyncD.Integration.Tests/PeopleSyncD.Integration.Tests.csproj --filter HrDemoSchemaTests`
Expected: PASS.

- [ ] **Step 3: Implement idempotent demo seed data, disabled in Production.**

Seed exactly 16 fictional workers, 4 departments, 2 locations, at least 2 managers, status mix across Active/Onboarding/Leave/Separated, 2 credentials expiring within 30 days, 1 expired credential, 2 incomplete onboarding instances, 2 open HR cases, document metadata, and audit activity. Use `.test` email addresses. Gate the seeder with `IHostEnvironment.IsDevelopment()` or an explicit demo environment flag that is false in Production.

- [ ] **Step 4: Implement report endpoints and reports page.**

```text
GET /api/v1/hr/reports/workforce-summary
GET /api/v1/hr/reports/credential-risk
```

- [ ] **Step 5: Verify and commit.**

```bash
dotnet test tests/PeopleSyncD.Api.Tests/PeopleSyncD.Api.Tests.csproj --filter HrReportingApiTests
dotnet test tests/PeopleSyncD.Integration.Tests/PeopleSyncD.Integration.Tests.csproj --filter HrDemoSchemaTests
cd src/PeopleSyncD.Web && npm run typecheck && npm run build
cd ../..
git add src tests
git commit -m "feat: add HR reports schema and demo dataset"
```

---

### Task 7: Demo Shell, Rehearsal, and Release Evidence

**Files:**

- Modify: `src/PeopleSyncD.Web/app/page.tsx`
- Modify: `src/PeopleSyncD.Web/app/layout.tsx`
- Modify: `src/PeopleSyncD.Web/app/globals.css`
- Create: `docs/demo/PEOPLESYNCD-HR-DEMO-RUNBOOK.md`
- Create: `tests/acceptance/hr-demo-smoke.md`

**Interfaces:**

Navigation order: Dashboard, People, Onboarding, Credentials, HR Cases, Reports, Security/Account.

Rehearsal path: sign in -> dashboard -> create employee -> employee profile -> onboarding task -> credential -> document metadata -> HR case -> employment status -> activity -> reports.

- [ ] **Step 1: Build a clear non-production HR demo shell.**

Landing page copy must identify the environment as `PeopleSyncD HR Demo — Non-Production` and link into the authenticated HR workspace. Keep current auth/security routes intact.

- [ ] **Step 2: Run complete local quality gates.**

```bash
dotnet test PeopleSyncD.slnx --configuration Release
cd src/PeopleSyncD.Web
npm ci
npm run typecheck
npm run build
npm audit --audit-level=high
```

Expected: zero failing tests; successful typecheck/build; no high-severity npm audit failure.

- [ ] **Step 3: Validate existing Codespaces or persistent Compose deployment path.**

Do not invent a new hosting architecture. Confirm API, web, gateway, and PostgreSQL start; demo seed executes only in the demo/development environment; browser reaches one HTTPS origin.

- [ ] **Step 4: Execute the complete rehearsal twice from a clean browser session.**

Pass criteria: no database editing, no code changes, no broken navigation, no cross-tenant leakage, and the employee created during rehearsal remains after reload.

- [ ] **Step 5: Write the demo runbook.**

Include exact launch command/path, environment label, sign-in procedure, deterministic seed/reset procedure, primary rehearsal path, expected screens, and fallback launch steps using the repository's existing deployment modes.

- [ ] **Step 6: Commit and push final head.**

```bash
git add src tests docs/demo
git commit -m "feat: finalize PeopleSyncD HR demo readiness"
git push origin demo/hr-mvp-readiness
```

- [ ] **Step 7: Require green GitHub Actions before declaring readiness.**

Do not disable or bypass failing .NET platform, API contracts, security, governance/traceability, Markdown, build/container, or applicable deployment gates. Fix failures on this branch and rerun them.

## Final Acceptance Checklist

- [ ] Existing authentication, MFA/passkeys, tenant selection, and session security pass.
- [ ] Executive dashboard is populated from persisted HR rows.
- [ ] Employee create/edit/status changes persist and survive reload.
- [ ] Invalid employment transitions are rejected.
- [ ] Versioned onboarding template v1 instantiates seven persisted tasks.
- [ ] Onboarding progress updates employee and dashboard views.
- [ ] Credential risk is date-derived and consistent in dashboard/report.
- [ ] HR cases require dedicated permissions and reject cross-tenant access.
- [ ] Document metadata persists and metadata-only records are labeled.
- [ ] Employee activity is backed by persisted audit records.
- [ ] Workforce Summary and Credential Risk reports reflect stored data.
- [ ] Demo seed is fictional, deterministic, idempotent, and disabled in Production.
- [ ] Clean PostgreSQL migration test passes.
- [ ] Next.js typecheck/build pass.
- [ ] Full .NET solution tests pass.
- [ ] Required GitHub Actions are green on the final head.
- [ ] Complete demo path succeeds twice without manual database intervention.

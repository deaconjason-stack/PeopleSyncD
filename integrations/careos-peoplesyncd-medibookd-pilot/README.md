# CareOS → PeopleSyncD → MediBookD: isolated fictional proof-of-flow

Status: **prototype / NOT a production integration / NOT approved payroll software**.

This pilot establishes an inspectable **adapter contract**, not a live connection. It was prepared after confirming the real PeopleSyncD repository and inspecting the existing Neon CareOS visit and audit table structures. MediBookD's actual repository and live API were not located or verified; therefore its role is represented here by an in-memory double-entry ledger adapter. Nothing here connects to a real database, employee, bank, processor, or client.

## Pipeline

1. **CareOS contract**: `agencyId`, `id`, `clientId`, `caregiverId`, `startAt`, `endAt`, `state='completed'`, `approved=true`. Note: `approved` is *not* present in the inspected CareOS `visits` table and **must be provided by a separately authorized timesheet approval workflow**. This pilot must never infer approval from `state='completed'` alone.
2. **PeopleSyncD contract**: separately tenant-verified staff record and hourly rate (integer US cents). Returns synthetic hours and **gross-wage-only preview**, never a legal paycheck. Weekly minutes > 40 hours are blocked pending verified workweek, overtime and wage rules, including cross-midnight and differential handling.
3. **MediBookD contract**: draft invoice (not sent), journal showing AR/service revenue and accrued wage expense/wages payable; ledger balances per source event. Taxes, payments, refunds and real GL account mapping are **not implemented**.
4. **Audit**: per-agency SHA-256 linked events, simulated only. This is tamper-evident within the generated output, **not** a durable, immutable production audit service.

## Run without credentials, dependencies, or network

Requires Node.js 22+. From this directory:

```bash
node --test pilot.test.mjs
```

All IDs are deliberately fictional; tests use fixed UTC dates and fictitious pay amounts.

## Before any real connection

- Identify and inspect MediBookD's actual source/API, and align contracts with PeopleSyncD's current .NET 9 architecture and CareOS's actual verified schema.
- Establish dedicated test-only tenant and resources **without copying production data**. Ensure authorization, MFA, least privilege, encryption, role mapping and audit verification, including denial tests across tenants.
- Implement signed, idempotent, authenticated service-to-service messages (or a controlled internal service) using server-side principal authorization. Never trust `agencyId` supplied by the browser alone.
- Build a **human-approved** timesheet source, caregiver verification, break handling, workweek/overtime/differentials, wage deductions, payroll taxes, state and federal filings, pay-stub and W-2 controls, reconciliation and employer-funded payments under a qualified payroll review.
- Verify client rate/authorization and actual delivered services before creating or sending invoices. Handle partial billings, credits, changes and failed exports.
- Require independent security/privacy, labor/payroll and accounting review before importing real W-2 employee data, granting financial permissions or enabling payment.

**No live payroll, production schemas, deployment, or secrets were touched.**

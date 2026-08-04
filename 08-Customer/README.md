# 08 — Customer

**Domain ID:** PSD-DOM-CUST-008  
**Accountable function:** Customer Implementation, Success, Training, and Support  
**Purpose:** Convert verified product capabilities into safe implementation, migration, adoption, training, support, and measurable customer outcomes.

## Canonical sources

- `SUPPORT.md`
- `SUPPORTED.md`
- `docs/releases/`
- `docs/operations/`
- `release/`
- Deployment and migration documentation
- Approved end-user, administrator, implementation, and support guides under this domain

## Required artifacts

- Customer qualification and readiness assessment
- Implementation methodology, roles, timeline, risks, and acceptance criteria
- Data migration mapping, validation, reconciliation, rollback, and sign-off
- Administrator, end-user, trainer, and support learning paths
- Professional-services scope and change-control process
- Support tiers, severity, escalation, communications, and incident procedures
- Adoption, utilization, outcome, renewal, and customer-health measures
- Supported-version and end-of-life policies

## Interfaces

Customer operations receives verified releases, deployment profiles, known issues, runbooks, product documentation, contracts, and customer requirements. It emits implementation evidence, support data, adoption outcomes, incident records, and governed feedback to Product and Business.

## Rules

- Customers may receive only capabilities approved for their deployment profile and intended use.
- Migration and implementation must preserve tenant boundaries, auditability, privacy, and rollback.
- Training must distinguish guidance, configuration authority, and actions requiring approval.
- Support diagnostics must be sanitized and privacy safe.
- Customer commitments and documentation must match verified release behavior.
- Feedback enters the governed backlog with source, impact, and decision history.

## Completion gate

A customer deployment is ready only when environment, identity, data, migration, security, training, support, acceptance, rollback, and ownership plans are approved and verified for that customer context.

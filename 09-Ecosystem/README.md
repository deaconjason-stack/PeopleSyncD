# 09 — Ecosystem

**Domain ID:** PSD-DOM-ECO-009  
**Accountable function:** Developer Platform, Partnerships, and Marketplace Governance  
**Purpose:** Enable governed third-party integrations, SDKs, partners, applications, certifications, and marketplace distribution.

## Canonical sources

- `packages/sdk/`
- `docs/api/`
- `docs/asyncapi/`
- `docs/licensing/`
- `docs/specifications/PSD-PEP-280-licensing-service.md`
- Integration Hub and partner documentation maintained under this domain

## Required artifacts

- Public and partner API portfolio
- SDK versioning, compatibility, generation, and support policy
- Developer onboarding, examples, sandbox, and testing guidance
- Partner tiers, qualification, obligations, benefits, and review process
- Marketplace submission, security, privacy, quality, billing, and removal policies
- Integration certification criteria and evidence
- OAuth, webhook, event, rate-limit, tenancy, and error-handling standards
- Commercial licensing and revenue-sharing rules

## Rules

- External integrations use approved contracts and least-privilege scopes.
- Partner applications cannot bypass tenant, identity, permission, audit, privacy, licensing, or AI-governance controls.
- Marketplace approval requires security, privacy, compatibility, support, and commercial review.
- SDK behavior must remain traceable to versioned source contracts.
- Deprecation requires notice, migration guidance, compatibility evidence, and end-of-support dates.
- Certification status must identify exact version, scope, evidence, and expiration.

## Completion gate

An integration or marketplace offering is releasable only when ownership, contracts, authentication, authorization, tenant isolation, privacy, support, compatibility, commercial terms, monitoring, suspension, and removal behavior are approved and tested.

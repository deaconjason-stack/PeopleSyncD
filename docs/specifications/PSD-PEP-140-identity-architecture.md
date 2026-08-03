---
id: PSD-PEP-140
title: PeopleSyncD Identity Architecture
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: Enterprise Architecture Office
approver: Jason Henderson
review_cycle: Annual
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-110
  - PSD-PEP-120
  - PSD-PEP-130
---

# PeopleSyncD Identity Architecture

## Purpose

Define authentication, digital identity, organization membership, authorization inputs, lifecycle, federation, and recovery across PeopleSyncD.

## Identity separation

PeopleSyncD separates:

- Person: human domain identity
- User: authentication identity
- Organization Membership: permission to enter a tenant
- Worker Assignment: workforce relationship
- Board Appointment: governance authority
- Service Identity: non-human workload identity

No relationship automatically grants another.

## Authentication capabilities

Target capabilities include passwords with adaptive hashing, passkeys, authenticator applications, security keys, recovery codes, enterprise SSO, OAuth 2.0 and OpenID Connect, SAML federation, and SCIM lifecycle integration.

Actual enabled methods are declared per edition and deployment and require implementation evidence.

## Multi-factor authentication

MFA is required for privileged roles. Fresh authentication is required for permission changes, confidential exports, board changes, security settings, signing operations, highly confidential documents, AI high-impact approvals, and other policy-defined actions.

## Session model

Sessions record user, device, authentication strength, organization context, creation, expiration, last activity, risk state, and revocation. Refresh credentials rotate and are invalidated on logout, password reset, suspected compromise, role removal, membership removal, or account disablement.

## Organization entry

A user may select only organizations with active membership. Switching organizations establishes a new explicit authorization context and is audited. Tenant context is not accepted solely from a client header.

## Authorization inputs

The Identity service supplies verified identity and authentication assurance. The Permissions service evaluates role, permission, tenant, relationship, classification, action, conditions, temporary grants, and approvals.

## Account lifecycle

Lifecycle states include invited, pending verification, active, locked, suspended, disabled, and archived. Offboarding revokes sessions, keys, recovery methods, memberships, and integration tokens according to workflow.

## Recovery and administration

Recovery paths are rate-limited, monitored, resistant to social engineering, and auditable. Administrative resets require reason, authority, notification where appropriate, and elevated verification.

## Service identities

Workloads use short-lived credentials and narrowly scoped permissions. Shared service accounts and long-lived embedded secrets are prohibited.

## Privacy

Authentication logs and device information are minimized, access-controlled, retained by policy, and not exposed through ordinary worker profiles.

## Verification

Tests cover credential handling, MFA, recovery, session rotation, revocation, organization switching, federation assertions, SCIM changes, disabled accounts, privilege changes, and audit evidence.

# PSD-PEP-710 — Identity and Session Hardening

- **Status:** Implemented foundation
- **Phase:** 7B1
- **Release line:** 0.3.1 identity beta
- **Owner:** Identity and Security Offices
- **Classification:** Commercial Confidential

## 1. Purpose

This specification establishes persistent identity authority for PeopleSyncD. Signed bearer tokens remain integrity envelopes, but they are not sufficient by themselves. Every protected request must also prove an active server-side session and current organization membership.

## 2. Implemented scope

Phase 7B1 implements:

1. persistent users and organization memberships;
2. tenant-scoped server-side sessions;
3. immediate session revocation and logout;
4. current membership and permission revalidation on every protected request;
5. pending TOTP and WebAuthn enrollment records without returning credential secrets;
6. an external-identity mapping schema for later OIDC or SAML integration;
7. append-only identity security events;
8. forced row-level security on tenant identity tables;
9. Founder membership seeded from governed authority; and
10. API, PostgreSQL integration, build, and Windows packaging evidence.

## 3. Authorization sequence

Every protected request shall execute the following sequence:

1. verify the token signature, shape, and expiry;
2. require an organization identifier authorized by the token;
3. retrieve the current active membership for the user and organization;
4. verify that the server-side session is active, unexpired, and unrevoked;
5. replace token permissions with current membership permissions;
6. enforce the required permission; and
7. generate or preserve a correlation identifier.

Suspending membership, ending membership, removing permission, or revoking the session therefore takes effect without waiting for token expiry.

## 4. Session controls

Sessions contain an immutable identifier, organization, user, authentication methods, issue time, expiry time, and optional revocation evidence.

A logout operation revokes the current session. Authorized session-revocation operations are limited to sessions belonging to the current user in the current organization in this increment.

## 5. MFA boundary

Phase 7B1 creates governed enrollment records only. It does not generate, store, reveal, or verify TOTP secrets, WebAuthn credentials, recovery codes, or challenge responses.

MFA may not be represented as active until a later verification ceremony proves possession and records approved verification evidence.

## 6. Federation boundary

The `external_identities` schema reserves immutable issuer and subject mapping. No public OIDC, OAuth2, SAML, or SCIM login flow is implemented or certified in this increment.

## 7. Security events

Session issuance, session revocation, and MFA enrollment initiation generate append-only security events with tenant, user, correlation, outcome, and limited metadata. Secrets, token values, authenticators, and recovery material are prohibited from security-event metadata.

## 8. Database controls

Organization memberships, sessions, MFA methods, and security events are protected by enabled and forced row-level security. The constrained runtime role receives only required current privileges.

The application still uses a migration-authorized connection that assumes the runtime role. Separate migration and deployed-runtime login credentials remain a certification requirement.

## 9. Verification

Automated verification covers:

- schema migration and Founder membership seed;
- forced RLS assertions;
- strict TypeScript;
- signed token shape and tamper rejection;
- current membership authority;
- logout and immediate revocation;
- persisted PostgreSQL session revocation;
- pending MFA enrollment records;
- tenant-scoped platform persistence;
- application compilation;
- SBOM and checksums; and
- unsigned Windows installer packaging.

## 10. Incomplete controls

The following remain outside this implemented foundation:

- OIDC and SAML protocol validation;
- production identity-provider configuration;
- MFA challenge, verification, recovery, reset, and step-up enforcement;
- recovery-code protection;
- WebAuthn attestation and origin validation;
- session rotation, refresh-token families, and trusted-device governance;
- administrator user and membership lifecycle APIs;
- separate deployed runtime database credentials;
- independent penetration testing; and
- production certification and signed distribution.

## 11. Release truthfulness

PeopleSyncD 0.3.1 is an unsigned identity-beta engineering artifact. It is not approved for public or customer production deployment and makes no healthcare, government, security, privacy, or accessibility certification claim.

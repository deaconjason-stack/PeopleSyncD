# PSD-PEP-730 — Recovery, Replay, and Administrative Safety

- **Status:** Implemented increment
- **Phase:** 7B2B1
- **Release line:** 0.4.1 recovery safety
- **Owner:** Identity and Security Offices
- **Classification:** Commercial Confidential

## 1. Purpose

This specification hardens PeopleSyncD identity recovery and administrative authority. It prevents reuse of TOTP time-step credentials, makes recovery codes single-use, protects the final active Founder authority, and requires a dedicated deployed-runtime database identity in production.

## 2. Implemented controls

Phase 7B2B1 implements:

1. monotonic TOTP counter persistence;
2. rejection of a TOTP counter that was already accepted;
3. one-time recovery-code consumption;
4. immediate recovery-session rotation;
5. append-only events for replay denial and recovery use;
6. application and database enforcement of the last-Founder invariant;
7. immediate revocation for suspended or ended non-Founder memberships;
8. a NOLOGIN shared capability role plus separately provisioned application login;
9. production configuration requiring `PEOPLESYNCD_RUNTIME_DATABASE_URL` and direct role mode; and
10. automated API, cryptographic, PostgreSQL, configuration, build, and packaging verification.

## 3. TOTP replay control

A successful TOTP verification records the accepted moving-factor counter. A later verification is denied when its matched counter is less than or equal to the stored counter. The denial generates `MFA_TOTP_REPLAY_DENIED` without recording the code or authenticator secret.

The counter is updated in the same tenant-scoped transaction that activates the method and rotates the authenticated session.

## 4. Recovery-code consumption

Recovery codes are displayed once when the first TOTP method is activated. Only keyed hashes are persisted.

Consumption requires:

- current authenticated organization context;
- the `identity.mfa.recovery.consume` permission;
- an unused matching hash; and
- an active server-side session.

The matched record is marked used, the current session is replaced atomically, and the replacement session records `recovery_code` as an authentication method. Reuse is denied.

This is a controlled step-up recovery mechanism. Unauthenticated account recovery, MFA reset, identity proofing, and support-assisted recovery remain outside this increment.

## 5. Last-Founder invariant

An organization may not suspend, end, or remove membership-management authority from its final active Founder.

The invariant is enforced twice:

1. the identity service checks for another active Founder with `organization.membership.manage`; and
2. a PostgreSQL trigger rejects a conflicting update even if application enforcement is bypassed.

This increment does not yet implement multi-party approval or separation-of-duties workflows for Founder changes.

## 6. Runtime database identity

`peoplesyncd_runtime` remains a NOLOGIN capability role. Production deployments must create a separate LOGIN role, grant it `peoplesyncd_runtime`, and provide that login through `PEOPLESYNCD_RUNTIME_DATABASE_URL`.

Migration credentials must not be supplied to the running API. Controlled development and CI may continue using owner credentials with `PEOPLESYNCD_DATABASE_ROLE_MODE=assume`.

## 7. Security events

The following events are added or strengthened:

- `MFA_TOTP_REPLAY_DENIED`;
- `MFA_RECOVERY_CODE_USED`;
- `MFA_RECOVERY_SESSION_ROTATED`; and
- `MEMBERSHIP_UPDATED`.

Events exclude TOTP values, recovery codes, authenticator secrets, session tokens, database passwords, and encryption keys.

## 8. Verification

Automated verification covers:

- RFC-compatible TOTP counter matching;
- monotonic counter persistence;
- same-counter replay denial;
- successful single-use recovery consumption;
- denial of recovery-code reuse;
- session replacement and old-token invalidation;
- application and database last-Founder enforcement;
- non-Founder suspension and session revocation;
- production runtime-database configuration enforcement;
- migration and forced-RLS assertions;
- strict TypeScript;
- application builds, SBOM, checksums; and
- unsigned Windows packaging.

## 9. Remaining boundaries

The following are not implemented or certified by Phase 7B2B1:

- WebAuthn registration or assertion ceremonies;
- unauthenticated account recovery;
- recovery-code replacement and MFA reset approval;
- adaptive risk scoring and edge rate limiting;
- production OIDC, OAuth2, SAML, or SCIM;
- multi-party Founder changes and separation of duties;
- independent penetration testing;
- accessibility, performance, resilience, backup, or disaster-recovery certification;
- Authenticode signing; and
- customer production approval.

## 10. Release truthfulness

PeopleSyncD 0.4.1 is an unsigned internal engineering artifact. It is not certified for public, healthcare, government, or customer production deployment.

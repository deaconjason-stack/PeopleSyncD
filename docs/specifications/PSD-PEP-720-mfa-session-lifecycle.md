# PSD-PEP-720 — MFA and Session Lifecycle Hardening

- **Status:** Implemented increment
- **Phase:** 7B2A
- **Release line:** 0.4.0 identity hardening
- **Owner:** Identity and Security Offices
- **Classification:** Commercial Confidential

## 1. Purpose

This specification advances PeopleSyncD from enrollment-record identity foundations to a verified TOTP ceremony, protected recovery-code foundation, atomic session rotation, and governed membership lifecycle.

## 2. Implemented scope

Phase 7B2A implements:

1. RFC 6238-compatible TOTP generation and verification using HMAC-SHA-1;
2. random TOTP secrets provided once through a standards-compatible provisioning URI;
3. AES-256-GCM protection of TOTP secrets at rest;
4. a dedicated production MFA encryption-key requirement;
5. failed-verification counters and append-only security events;
6. activation of a pending TOTP method only after successful possession proof;
7. one-time display of recovery codes with only keyed hashes stored;
8. atomic session rotation with stable session-family identifiers;
9. immediate invalidation of replaced session tokens;
10. current membership and permission revalidation on every protected request;
11. governed membership status and permission administration; and
12. immediate session revocation when membership is suspended or ended.

## 3. TOTP enrollment and verification

TOTP enrollment creates a pending MFA method and a cryptographically random secret. The plaintext secret is returned only inside the provisioning URI during enrollment. The database stores only authenticated ciphertext.

Verification accepts a six-digit TOTP within the approved clock window. A failed verification increments failure evidence and records a denied security event. A successful verification activates the method, rotates the current session, adds `totp` to the authentication-method evidence, generates recovery codes, and records a successful security event.

## 4. Recovery-code boundary

Recovery codes are returned once following successful TOTP activation. Only keyed HMAC-SHA-256 hashes are stored in the tenant-scoped recovery-code table.

Recovery-code consumption, replacement, and administrative reset are not implemented in this increment. The stored records are a protected foundation and may not be represented as a complete account-recovery capability.

## 5. Session rotation

Every session has a session-family identifier. Rotation executes in one database transaction:

1. lock and validate the current active session;
2. revoke the current session;
3. link the current session to its replacement;
4. create a replacement in the same family;
5. preserve approved authentication methods; and
6. append a correlated security event.

The prior token becomes unusable immediately because protected requests require an active server-side session.

## 6. Membership lifecycle

An authorized membership administrator may change a membership status and its permission set. Suspending or ending membership revokes all active sessions for that user in the organization. Protected requests also reload current membership authority, so permission removal takes effect without waiting for token expiry.

A last-Founder protection rule and broader delegated-administration workflow remain future governance controls.

## 7. Database controls

The following tenant data is protected by enabled and forced row-level security:

- organization memberships;
- identity sessions;
- MFA methods;
- MFA recovery-code hashes; and
- identity security events.

The runtime role receives only the required table privileges. Separate deployed-runtime and migration login credentials remain required before certification.

## 8. Verification

Automated verification covers:

- RFC 6238 test vectors;
- authenticated encryption round trips and wrong-key rejection;
- deterministic invalid-code denial;
- successful TOTP activation;
- recovery-code count and non-disclosure after creation;
- atomic session-family rotation;
- invalidation of old tokens;
- live PostgreSQL persistence;
- membership suspension and immediate revocation;
- schema, permission, and forced-RLS assertions;
- strict TypeScript, application builds, SBOM, checksums, and unsigned Windows packaging.

## 9. Incomplete controls

The following remain outside Phase 7B2A:

- WebAuthn registration and assertion ceremonies;
- TOTP replay-counter persistence and distributed clock monitoring;
- recovery-code consumption and replacement;
- account recovery and MFA reset approval workflows;
- production OIDC, OAuth2, SAML, and SCIM integration;
- rate limiting and adaptive risk controls at the edge;
- last-Founder and separation-of-duties invariants;
- separate deployed-runtime database credentials;
- independent penetration testing; and
- signed, certified distribution.

## 10. Release truthfulness

PeopleSyncD 0.4.0 is an unsigned identity-hardening engineering build. It is not approved for public or customer production deployment and makes no healthcare, government, security, privacy, or accessibility certification claim.

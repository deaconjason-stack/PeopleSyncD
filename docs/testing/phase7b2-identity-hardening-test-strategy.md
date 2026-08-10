# Phase 7B2A Identity-Hardening Test Strategy

## Objective

Verify that PeopleSyncD 0.4.0 correctly protects TOTP credentials, rotates sessions atomically, enforces current membership authority, and preserves truthful release boundaries.

## Automated layers

### Authentication primitives

- Signed-session round trip and tamper rejection.
- RFC 6238 SHA-1 reference-vector verification.
- AES-256-GCM encryption and decryption.
- Wrong-key authenticated-decryption failure.

### API behavior

- Development session issuance remains disabled in production configuration.
- Explicit organization context remains mandatory.
- Session rotation preserves the family and rejects the prior token.
- TOTP enrollment begins pending and returns a provisioning URI once.
- Valid TOTP proof activates the method and produces an MFA-authenticated replacement session.
- Invalid proof remains pending and returns unauthorized.
- Recovery codes are returned only by successful verification.
- Membership suspension immediately denies the prior session.
- Existing board, audit, tenant, and Domonique safeguards remain unchanged.

### PostgreSQL integration

- Migrations apply in lexical order with `ON_ERROR_STOP`.
- Session-family, replacement-link, encrypted-secret, and recovery-code schema exists.
- Founder membership contains the controlled Phase 7B2A permissions.
- Forced row-level security is enabled on all platform and identity tenant tables.
- Session rotation is atomic and durable.
- Encrypted TOTP activation and recovery-code hashing persist successfully.
- Cross-tenant Person records remain isolated.

## Evidence gates

The authoritative pipeline shall run:

1. migrations;
2. identity seed and schema assertions;
3. strict TypeScript;
4. all unit and integration tests;
5. production builds;
6. CycloneDX SBOM generation;
7. SHA-256 checksum generation; and
8. unsigned Windows NSIS packaging.

## Manual verification still required

Before production certification, the program must separately complete:

- authenticator-app interoperability testing;
- clock-skew and replay-control testing;
- recovery-code consumption testing;
- WebAuthn browser and Windows ceremony testing;
- administrative separation-of-duties testing;
- accessibility and end-to-end browser testing;
- dependency and penetration testing;
- backup and disaster-recovery exercises; and
- signed installer acceptance.

## Exit rule

Phase 7B2A is considered verified only when all nine repository workflows pass on the same authoritative commit and the evidence artifacts identify that commit. Passing this strategy does not constitute production certification.

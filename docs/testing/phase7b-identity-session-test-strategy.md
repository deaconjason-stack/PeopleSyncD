# Phase 7B1 Identity and Session Test Strategy

## Objective

Verify that signed tokens, persistent memberships, and server-side session state work together to provide immediately revocable, tenant-scoped authorization.

## Automated coverage

The suite verifies:

1. token round-trip and tamper rejection;
2. organization and permission denial;
3. development session issuance from persisted membership authority;
4. immediate logout revocation;
5. persisted PostgreSQL session creation and revocation;
6. current membership retrieval;
7. pending MFA enrollment records without secret material;
8. platform tenant isolation;
9. identity and platform readiness;
10. application builds, SBOM, checksums, and installer packaging.

## Required CI database assertions

CI shall verify the Founder user and active Founder membership seed. It shall also verify forced row-level security on Person, Worker, Audit, Membership, Session, MFA, and Security Event tables.

## Negative boundaries

This suite does not certify:

- real OIDC or SAML provider login;
- TOTP verification;
- WebAuthn ceremonies;
- recovery codes or account recovery;
- step-up authentication;
- refresh-token rotation;
- independent penetration resistance; or
- production identity-provider configuration.

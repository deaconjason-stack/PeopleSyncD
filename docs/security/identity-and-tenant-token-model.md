# Identity and Tenant Token Model

## Token stages

PeopleSyncD uses a two-stage access model.

An authenticated user token establishes identity but carries no organization permissions. A tenant token is issued only after the platform verifies an active membership for the selected organization.

## Tenant token claims

Tenant-scoped tokens contain:

- `sub` for the platform user identifier.
- `membership_id` for the verified membership.
- `tenant_id` for the selected organization.
- `tenant_role` for the membership role.
- Repeated `permission` claims derived from the role catalog.
- Issuer, audience, issued-at, not-before, and expiration controls.

## Authorization boundary

A permission claim is necessary but not sufficient for object access. Tenant-bound controllers compare route or resource organization identifiers to the validated `tenant_id` claim. A mismatch is denied even when the token contains the requested permission.

## Signing keys

No JWT signing key is stored in source control. Development and test processes may generate an ephemeral key at startup. Production startup fails unless `Jwt:SigningKey` is supplied by a protected configuration provider and contains at least 256 bits of entropy.

## Browser storage boundary

The M2.1 demonstration workspace stores its short-lived token in `sessionStorage`, which clears with the browser session. A production browser release should move to a hardened backend-for-frontend design with secure, HTTP-only, same-site cookies, anti-forgery controls, refresh-token rotation, and content-security-policy enforcement.

## Deferred controls

- Verified email delivery and confirmation enforcement.
- MFA and phishing-resistant authentication.
- Refresh-token reuse detection and revocation.
- OIDC, SAML, and SCIM federation.
- Administrative invitation and membership lifecycle UI.
- Central signing-key rotation and hardware-backed key custody.

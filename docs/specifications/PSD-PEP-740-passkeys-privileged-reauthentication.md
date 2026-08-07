---
id: PSD-PEP-740
title: Passkeys and Privileged Reauthentication
version: 1.0.0
status: Implemented Foundation
classification: Commercial Confidential
owner: Identity and Security Offices
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-07
updated: 2026-08-07
supersedes: null
references:
  - PSD-PEP-140
  - PSD-PEP-710
  - PSD-PEP-720
  - PSD-PEP-730
  - PSD-REQ-PASSKEY-000001
  - PSD-REQ-REAUTH-000001
  - PSD-REQ-AUTHSESSION-000001
---

# PSD-PEP-740 — Passkeys and Privileged Reauthentication

## 1. Purpose

M2.4 adds a WebAuthn/passkey security foundation to the .NET identity runtime and binds sensitive identity and membership actions to a maximum authentication age. The implementation extends the existing PeopleSyncD session-assurance model rather than creating a parallel identity system.

## 2. WebAuthn boundary

PeopleSyncD integrates FIDO2/WebAuthn verification through the Fido2 4.0.1 library on the existing .NET 9 runtime. Relying-party ID, relying-party name, and accepted browser origins are configuration values. Production deployments must override development values with the exact production relying-party ID and HTTPS origins.

Registration requests:

- require WebAuthn user verification;
- request a resident/discoverable credential;
- exclude already registered credential IDs;
- persist only the public credential material required for later verification; and
- use attestation conveyance `none`, so this increment does not claim enterprise authenticator attestation or device provenance.

The authenticator retains the private key. PeopleSyncD does not receive or store WebAuthn private keys.

## 3. Passkey credential persistence

Each active passkey record stores:

- an opaque credential ID;
- the credential public key;
- the user handle;
- the signature counter supplied by WebAuthn;
- backup eligibility and backup state;
- AAGUID metadata;
- an operator-facing display name; and
- creation, last-use, and revocation timestamps.

Credential IDs are unique. Revocation is server-side and audited. Revoked credentials are excluded from subsequent assertion ceremonies.

## 4. Ceremony lifecycle

Registration, login, and step-up ceremonies are persisted with their original WebAuthn options and expire after five minutes by default. A ceremony is single-use. PostgreSQL uses a conditional completion update so concurrent replay cannot produce two successful completions.

The passwordless login flow is intentionally email-first in M2.4. PeopleSyncD identifies the account before creating assertion options and supplies only that account's active credential descriptors. This avoids introducing username-less account discovery semantics before a separate privacy review.

## 5. Phishing-resistant assurance

A verified passkey assertion issues a session with:

- `psd_assurance=phishing_resistant`;
- `amr=passkey`;
- a server-backed refresh-session family; and
- the actual authentication instant in `auth_time`.

Phishing-resistant assurance satisfies existing MFA-required tenant boundaries without being downgraded to TOTP assurance.

## 6. Authentication freshness

Refresh-token rotation may extend a session family but must not advance `auth_time` or rewrite the authentication method. The original authentication instant and method are persisted in the refresh family and copied unchanged during rotation.

Only a real password, MFA, or passkey authentication ceremony can create a new authentication instant.

## 7. Privileged-operation boundary

The initial M2.4 privileged-operation policy requires authentication no more than five minutes old for:

- passkey registration;
- passkey revocation;
- organization invitations; and
- organization membership lifecycle or role changes.

Existing MFA recovery-code regeneration remains separately protected by MFA assurance. Future increments may bind additional high-impact actions and may require phishing-resistant assurance rather than only freshness.

## 8. Browser integration

The Next.js client converts WebAuthn base64url challenge and credential fields into browser `ArrayBuffer` values and serializes attestation/assertion responses back to the server contract. Browser support is feature-checked before invoking `navigator.credentials.create` or `navigator.credentials.get`.

## 9. Verification

Automated M2.4 verification covers:

- deterministic NuGet restore including Fido2 4.0.1;
- strict .NET formatting and analyzer build;
- registration options that require user verification and resident credentials;
- generic failure for unknown/unregistered passkey login;
- rejection of stale authentication at passkey enrollment;
- authentication-time preservation across refresh rotation;
- PostgreSQL passkey and ceremony schema assertions;
- Next.js typecheck and production build;
- high-severity npm audit;
- Docker Compose validation; and
- API and web image builds.

CI does not possess a physical platform authenticator or security key. Therefore this increment does not claim automated hardware-in-the-loop WebAuthn acceptance testing.

## 10. Remaining boundaries

M2.4 does not certify:

- customer production deployment;
- production RP/origin configuration;
- enterprise attestation or authenticator provenance policy;
- hardware-in-the-loop browser acceptance;
- account recovery through passkeys;
- support-assisted credential recovery;
- production browser refresh-token custody;
- adaptive risk scoring;
- independent penetration testing;
- signed release packaging; or
- regulatory approval.

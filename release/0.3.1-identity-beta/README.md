# PeopleSyncD 0.3.1 Identity Beta

## Status

Phase 7B1 implementation evidence exists. This package is **unsigned, not production certified, and not approved for customer production deployment**.

## Implemented evidence

- persistent Founder user and organization membership;
- current membership and permission enforcement;
- server-side revocable sessions;
- immediate logout invalidation;
- tenant-protected session and MFA records;
- pending TOTP and WebAuthn enrollment records without secret output;
- append-only identity security events;
- external-identity database foundation;
- strict TypeScript and 18 automated tests at the first verified implementation head;
- SBOM, checksums, API/web artifacts, and unsigned Windows installer.

## Remaining gates

Real federation, MFA challenge verification and recovery, separate deployed runtime credentials, administrative identity lifecycle, browser and desktop end-to-end testing, accessibility, performance, backup restoration, disaster recovery, independent security testing, protected signing, and authorized certification remain incomplete.

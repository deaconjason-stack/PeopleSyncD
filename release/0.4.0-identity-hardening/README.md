# PeopleSyncD Genesis 0.4.0 Identity Hardening

This directory defines the controlled evidence boundary for the Phase 7B2A identity-hardening engineering build.

## Included scope

- encrypted TOTP enrollment and possession verification;
- hashed recovery-code foundation;
- atomic session-family rotation;
- current membership and permission enforcement;
- membership suspension and session revocation;
- PostgreSQL row-level security;
- automated tests, SBOM, checksums, and unsigned Windows packaging.

## Classification

- Engineering status: implemented increment
- Distribution status: internal verification only
- Code-signing status: unsigned
- Certification status: not certified
- Customer-production status: prohibited

## Evidence source

The GitHub Actions build manifest resolves the source commit and workflow run at build time. Downloaded artifacts must be matched to those values and their SHA-256 digest before use.

## Remaining boundaries

WebAuthn ceremonies, recovery-code consumption, federation, separate deployed-runtime database credentials, independent security testing, signed distribution, and production certification remain incomplete.

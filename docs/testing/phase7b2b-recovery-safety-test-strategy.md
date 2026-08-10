# Phase 7B2B1 Recovery and Administrative Safety Test Strategy

## Objective

Verify that recovery credentials are single-use, accepted TOTP counters cannot be replayed, final Founder authority cannot be removed, and production configuration requires a dedicated runtime database identity.

## Automated layers

### Cryptographic unit tests

- RFC 6238 code generation and verification.
- Matched moving-factor counter output.
- Invalid code returns no counter.
- Authenticated encryption round trip and wrong-key rejection.

### API tests

- Initial TOTP activation returns eight one-time recovery codes.
- The accepted TOTP counter cannot be reused.
- Recovery-code consumption rotates the active session.
- The consumed code cannot be reused.
- Final Founder suspension returns conflict and preserves access.
- Existing tenant, audit, board, session, and AI-tool protections remain green.

### PostgreSQL integration tests

- `last_totp_counter` persists in the MFA record.
- Same-counter replay is denied after the first commit.
- Recovery-code `used_at` is committed with session rotation.
- A consumed hash cannot be selected again.
- The database trigger rejects final Founder lockout.
- A non-Founder suspension revokes active sessions.
- Forced row-level security remains enabled on all protected tables.

### Configuration tests

- Production rejects a migration-only database URL.
- Production rejects assume-role mode.
- Production accepts a dedicated runtime URL with direct mode.
- Test and development environments retain controlled assume-role support.

### Build and packaging tests

- Strict TypeScript passes for every workspace.
- API, web, and desktop builds complete.
- SBOM and checksums are generated.
- The unsigned 0.4.1 Windows installer packages successfully.

## Required release evidence

A successful internal increment requires all repository gates, all tests, migration assertions, build evidence, and Windows packaging to pass on the same authoritative commit.

## Explicit exclusions

This strategy does not certify WebAuthn, unauthenticated account recovery, federated identity, independent penetration testing, accessibility, resilience, disaster recovery, or signed customer distribution.

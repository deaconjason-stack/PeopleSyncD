# PeopleSyncD Installation Guide

## Status

Foundation guide. No production package is currently certified.

## Required sequence

1. Verify the release version, source commit, checksums, signatures, SBOM, compatibility matrix, and certification status.
2. Prepare supported infrastructure, DNS, certificates, storage, database, identity provider, secret manager, backup, telemetry, and support contacts.
3. Create production secrets outside the repository.
4. Apply database migrations only after backup and rollback preparation.
5. Deploy digest-pinned signed artifacts through the approved package.
6. Verify health, tenant isolation, authentication, authorization, document storage, audit delivery, notifications, backup, and restore procedures.
7. Record the installation evidence and customer acceptance.

Deployment-model-specific instructions remain required before production use.

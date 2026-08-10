# PeopleSyncD .NET PostgreSQL Migration Contract

This directory contains the ordered PostgreSQL migration contract for the .NET platform implementation.

- `0001_m21_identity_foundation.sql` establishes the minimum identity, organization, and membership tables required by M2.1.
- `0002_m22_membership_security.sql` adds organization invitations, rotating refresh sessions, and security audit records for M2.2.

Local development currently uses EF Core `EnsureCreated` for fast disposable environments. These SQL files are the version-controlled database-change contract for deployment engineering and CI validation. A later release milestone will replace development bootstrap behavior with an explicit EF migration runner and release-approved database migration orchestration.

Never edit an already-released migration in place. Add a new forward migration and a documented recovery procedure.

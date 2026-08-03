# PeopleSyncD Upgrade Guide

1. Review release notes, compatibility, API and schema changes, known issues, security advisories, and deprecations.
2. Verify backups through restoration evidence.
3. Rehearse migrations and rollback in a representative non-production environment.
4. Confirm signed artifacts, SBOM, checksums, and release certification.
5. Drain or pause workflows requiring consistency.
6. Apply migrations and deploy immutable artifacts using staged rollout.
7. Run smoke, security, tenant-isolation, accessibility, and operational checks.
8. Resume traffic and monitor defined indicators.
9. Roll back or forward-fix according to the approved plan if gates fail.

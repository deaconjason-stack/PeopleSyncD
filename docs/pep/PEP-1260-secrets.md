# PEP-1260: Secrets

- Status: Accepted

Secrets are stored in an approved secret manager or protected deployment environment, never in source, logs, fixtures, images, desktop bundles, or documentation. Secrets are scoped, rotated, revocable, and auditable. CI scans for accidental exposure. Compromised secrets are revoked immediately and related incidents documented.

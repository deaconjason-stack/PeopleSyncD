# Deployment and Certification Test Strategy

## Required verification

- Docker Compose configuration and local dependency health
- Kubernetes schema, security context, resource, and network-policy validation
- Configuration-schema positive and negative tests
- Secret scanning and secret-rotation exercises
- Backup creation, integrity, isolated restoration, and validation
- Disaster-recovery failover, reconciliation, communications, and return to service
- Log, metric, trace, alert, and privacy tests
- Support-bundle allowlist and redaction tests
- Dependency, license, vulnerability, SBOM, checksum, provenance, and signature checks
- Installation, migration, upgrade, rollback, and compatibility tests
- Performance budgets under declared workloads
- Accessibility verification for core workflows
- Release-evidence completeness and contradiction checks

Every executable case receives a permanent `PSD-TEST-DEPLOY-*` or `PSD-TEST-CERT-*` identifier before production certification.

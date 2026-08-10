# PeopleSyncD Architecture Decision Records

Architecture Decision Records preserve why significant technical choices were made, the alternatives considered, consequences, migration strategy, and verification expectations.

## Initial Genesis decisions

| ID | Decision | Status |
| --- | --- | --- |
| PSD-ADR-000001 | Use PostgreSQL as the primary transactional database | Accepted |
| PSD-ADR-000002 | Preserve history through lifecycle state and controlled archival | Accepted |
| PSD-ADR-000003 | Use UUID identifiers for governed records | Accepted |
| PSD-ADR-000004 | Evaluate CQRS selectively rather than platform-wide | Proposed |
| PSD-ADR-000005 | Evaluate Kafka-compatible event streaming for enterprise deployments | Proposed |
| PSD-ADR-000006 | Restrict Redis to approved ephemeral workloads | Proposed |
| PSD-ADR-000007 | Standardize observability on OpenTelemetry | Accepted |
| PSD-ADR-000008 | Use Electron for the initial Windows desktop shell | Proposed |
| PSD-ADR-000009 | Define REST interfaces with OpenAPI | Accepted |
| PSD-ADR-000010 | Define asynchronous interfaces with AsyncAPI | Accepted |
| PSD-ADR-000011 | Consolidate the PR 1-9 lineage as the authoritative baseline | Accepted |

Accepted decisions govern future implementation but do not claim that implementation is complete.

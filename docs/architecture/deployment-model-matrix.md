# PeopleSyncD Deployment Model Matrix

| Capability | Cloud SaaS | Private Cloud | On-Premises | Hybrid | Government Restricted |
|---|---|---|---|---|---|
| Operations owner | MediSyncD | Shared or customer | Customer | Shared | Approved authority |
| Tenant isolation | Logical and service controls | Dedicated boundary | Customer boundary | Coordinated boundaries | Separately approved |
| Kubernetes target | Managed | Managed or dedicated | Supported distribution | Multiple clusters | Approved distribution |
| Secrets | Managed service | Dedicated manager | Customer manager | Federated controls | Approved manager |
| Backup | Managed | Dedicated | Customer-operated | Coordinated | Approved storage |
| Telemetry | Managed | Customer-controlled options | Customer-controlled | Federated | Restricted |
| Upgrades | Managed rollout | Coordinated | Customer scheduled | Coordinated | Change-controlled |
| Certification | Release plus service evidence | Release plus environment evidence | Release plus installation evidence | Combined evidence | Separate authorization |

This matrix defines target responsibility boundaries and does not certify any environment.

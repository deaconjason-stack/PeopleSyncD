# HR Domain Data Ownership

| Data | Owning service |
|---|---|
| Person master and contact history | Person |
| Worker and employment relationships | Employment / Worker |
| Onboarding templates and instances | Onboarding |
| Credentials and training evidence | Credentials |
| Restricted HR cases | HR Cases |
| Authentication and sessions | Identity |
| Organization hierarchy and membership | Organization |
| Files and versions | Documents |
| Approvals and transitions | Workflow |
| Immutable activity evidence | Audit |
| Board appointments and governance | Board |

Cross-service references use stable identifiers and governed contracts. Services must not directly mutate another service's owned records.

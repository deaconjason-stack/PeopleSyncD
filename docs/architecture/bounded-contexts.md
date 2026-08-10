# PeopleSyncD Bounded Contexts

PeopleSyncD uses explicit bounded contexts to prevent one application from becoming an ungoverned monolith.

| Context | Primary ownership | Representative responsibilities |
| --- | --- | --- |
| Identity | Identity Domain | users, credentials, sessions, federation, recovery |
| Organizations | Platform Team | tenants, hierarchy, memberships, settings |
| Permissions | Platform Security | roles, permissions, policies, grants, decisions |
| Human Resources | HR Domain | workers, assignments, employment lifecycle, onboarding |
| Governance | Board Domain | appointments, meetings, resolutions, votes, disclosures |
| Documents | Platform Team | metadata, versions, storage, classification, retention |
| Workflow | Platform Team | state machines, tasks, approvals, deadlines, exceptions |
| Notifications | Platform Team | templates, preferences, delivery, retries |
| Audit | Security Office | append-only evidence, integrity, authorized retrieval |
| Search | Platform Team | tenant-aware indexing and retrieval |
| Reporting | Analytics Team | governed reports, metrics, exports, warehouse products |
| Licensing | Commercial Platform | editions, entitlements, activation, limits |
| Configuration | Platform Team | validated and versioned tenant and application settings |
| Integrations | Integration Team | external connectors, webhooks, import, export |
| Domonique 2.0 | AI Platform Office | conversations, tools, retrieval, approvals, AI audit |

Cross-context communication uses approved APIs and versioned events. Direct database access across context ownership requires an approved ADR and migration plan.

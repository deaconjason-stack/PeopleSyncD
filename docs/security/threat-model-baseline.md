# PeopleSyncD Threat Model Baseline

## Protected assets

- authentication credentials and sessions
- tenant and organization boundaries
- workforce and applicant records
- board and governance records
- documents, signatures, retention, and legal holds
- permissions and security configuration
- audit evidence
- licensing and entitlement records
- Domonique 2.0 conversations, tools, memory, and source records
- source code, build systems, signing keys, and deployment credentials

## Trust boundaries

- user device to PeopleSyncD client
- client to API platform
- API platform to services
- service to database, cache, search, storage, and event systems
- PeopleSyncD to external identity and integration providers
- Domonique 2.0 to model providers and approved tools
- operational staff to customer environments

## Priority threats

- account takeover and recovery abuse
- privilege escalation and permission drift
- cross-tenant direct object access
- malicious document upload and content injection
- SQL, command, template, and query injection
- compromised dependency or build pipeline
- secret and signing-key exposure
- audit deletion or modification
- event spoofing, replay, and duplicate processing
- insecure desktop update or local storage
- support-account misuse
- data export and backup leakage
- prompt injection and retrieval poisoning
- unauthorized AI tool execution
- model-provider retention or training on customer data

## Required treatment

Each service threat model identifies applicable threats, preventive controls, detective controls, response, residual risk, and automated verification. Critical unresolved threats block release.

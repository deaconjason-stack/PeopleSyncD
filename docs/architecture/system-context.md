# System Context

Users access PeopleSyncD through web, Windows desktop, and future mobile clients. An API gateway routes authenticated requests to organization, worker, board, documents, workflow, audit, notifications, and AI services. PostgreSQL stores service data; protected object storage holds document bytes; an event bus carries minimized versioned events; telemetry and backup systems support operations.

The active organization is always explicit. Cross-tenant access fails closed. High-impact AI-prepared actions require human approval and audit evidence.

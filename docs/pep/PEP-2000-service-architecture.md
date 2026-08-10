# PEP-2000: Service Architecture

- Status: Accepted

PeopleSyncD uses contract-first service boundaries for worker, organization, board, documents, workflow, audit, notifications, and AI orchestration. Each service owns its domain logic and data contracts, enforces tenant scope and authorization, emits versioned events, records audit evidence, publishes health and telemetry, and provides migration and test documentation.

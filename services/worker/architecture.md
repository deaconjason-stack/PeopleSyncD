# Worker Service Architecture

## Components

- HTTP adapter validates OpenAPI requests and identity context.
- Authorization adapter evaluates tenant, role, scope, relationship, and action.
- Application service enforces worker invariants and state transitions.
- Repository persists assignments using tenant-scoped SQL.
- Outbox publishes versioned worker events after committed changes.
- Audit adapter emits append-only evidence with correlation identifiers.

## Dependencies

Organization context, person reference validation, authorization policy, audit ingestion, notification delivery, and optional workflow integration.

## Failure behavior

Authorization and tenant ambiguity fail closed. Event publication uses an outbox so committed records are not lost when a broker is unavailable.

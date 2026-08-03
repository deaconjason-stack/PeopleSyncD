# Worker Database Contract

Canonical SQL is in `database/worker/`.

The service owns `worker_assignments` and related worker history views. `organization_id` is mandatory on every row and used by row-level security. Cross-service identities are stored as stable UUID references without bypassing service contracts. Ended assignments retain dates and status. Schema changes require migration, rollback, compatibility analysis, and traceability updates.

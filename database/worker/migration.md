# Worker Database Migration

## Order

1. `schema.sql`
2. `constraints.sql`
3. `indexes.sql`
4. `views.sql`

Production migrations must run through a versioned migration tool, inside controlled transactions where supported, with preflight checks, backup confirmation, compatibility testing, post-migration validation, and a documented rollback or forward-fix strategy.

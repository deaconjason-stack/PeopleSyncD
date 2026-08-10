# AI Database Migration

Apply `schema.sql`, `constraints.sql`, `indexes.sql`, and `views.sql` through a versioned migration tool only after security review.

Before production use, verify encryption keys, tenant policies, legal hold, retention jobs, backup and restore, rollback or forward-fix, and deletion behavior. These files are contracts, not evidence of an executed migration.

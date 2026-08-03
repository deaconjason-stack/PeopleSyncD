# Permissions Architecture

Components: policy API, policy evaluator, relationship resolver, classification resolver, grant store, policy cache, decision logger, and audit adapter.

Policy cache entries are tenant-scoped and invalidated on policy or membership change.

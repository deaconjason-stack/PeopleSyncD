# PEP-1170: Workflow

- Status: Accepted

Workflows model explicit states, transitions, actors, deadlines, approvals, exceptions, and audit evidence. State changes must be idempotent where possible and validated against current state. High-impact transitions require human approval. Failures must preserve the last valid state and permit safe retry, cancellation, or rollback.

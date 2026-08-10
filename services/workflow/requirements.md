# Workflow Requirements

- `PSD-REQ-WORKFLOW-000001` Deterministic workflow state transitions.
- High-impact transitions require authorized human approval.
- State changes are idempotent where required.
- Separation of duties prevents prohibited self-approval.
- Failures preserve the last valid committed state.

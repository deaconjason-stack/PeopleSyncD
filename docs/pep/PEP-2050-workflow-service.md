# PEP-2050: Workflow Service

- Status: Accepted

The Workflow service owns state machines, tasks, approvals, deadlines, exceptions, retries, and transition evidence for onboarding and other governed processes. It must enforce transition rules, idempotency, separation of duties where required, and human approval for high-impact actions. Initial requirement namespace: `REQ-WORKFLOW-*`.

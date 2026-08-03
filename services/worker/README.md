# Worker Service

The Worker service implements PEP-2010 and owns workforce assignments within an organization. It provides tenant-scoped APIs and events for worker creation, retrieval, status changes, supervisor relationships, and historical assignment records.

## Traceability

`PEP-2010 → REQ-WORKER-* → services/worker/api.yaml → database/worker/* → services/worker/tests.md → release evidence`

## Safety boundary

The service does not own authentication credentials, board authority, document bytes, payroll execution, or unrestricted identity documents. Every operation requires authenticated organization context, authorization, validation, and audit emission.

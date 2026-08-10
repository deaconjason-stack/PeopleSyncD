# Worker Service Requirements

- `PSD-REQ-HR-WORKER-000001` The service shall create a tenant-scoped worker assignment for an existing authorized Person and Organization.
- `PSD-REQ-HR-WORKER-000002` The service shall reject a record whose organization does not match the verified organization context.
- `PSD-REQ-HR-WORKER-000003` The service shall preserve assignment history instead of overwriting ended relationships.
- `PSD-REQ-HR-WORKER-000004` Directory responses shall omit highly confidential identity, tax, medical, accommodation, screening, and compensation fields.
- `PSD-REQ-HR-WORKER-000005` Worker status transitions shall be validated against an explicit state model.
- `PSD-REQ-HR-WORKER-000006` Every create, update, end, archive, denial, and failed transition shall emit audit evidence.
- `PSD-REQ-HR-WORKER-000007` Read operations shall enforce own, direct-report, department, organization, or explicitly granted scope.
- `PSD-REQ-HR-WORKER-000008` Writes shall use optimistic concurrency or an equivalent lost-update control.
- `PSD-REQ-HR-WORKER-000009` API and event contracts shall be versioned and compatibility assessed before release.
- `PSD-REQ-HR-WORKER-000010` The service shall remain operable when Domonique 2.0 is disabled.

The master machine-readable registry currently defines `PSD-REQ-HR-WORKER-000001`. Requirements 000002 through 000010 are service-level draft requirements pending registry promotion and Product Office approval.

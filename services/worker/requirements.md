# Worker Service Requirements

- `REQ-WORKER-0001` The service shall create a tenant-scoped worker assignment for an existing authorized person and organization.
- `REQ-WORKER-0002` The service shall reject a record whose organization does not match the authenticated organization context.
- `REQ-WORKER-0003` The service shall preserve assignment history instead of overwriting ended relationships.
- `REQ-WORKER-0004` Directory responses shall omit highly confidential identity, tax, medical, accommodation, screening, and compensation fields.
- `REQ-WORKER-0005` Worker status transitions shall be validated against an explicit state model.
- `REQ-WORKER-0006` Every create, update, archive, denial, and failed transition shall emit audit evidence.
- `REQ-WORKER-0007` Read operations shall enforce own, direct-report, department, organization, or explicitly granted scope.
- `REQ-WORKER-0008` Writes shall use optimistic concurrency or an equivalent lost-update control.
- `REQ-WORKER-0009` API and event contracts shall be versioned and backward compatibility assessed before release.
- `REQ-WORKER-0010` The service shall remain operable when Domonique 2.0 is disabled.

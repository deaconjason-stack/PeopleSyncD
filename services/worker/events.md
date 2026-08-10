# Worker Events

Canonical event contracts are in `services/worker/asyncapi.yaml` and `docs/asyncapi/worker-events.yaml`.

Events:

- `worker.created.v1`
- `worker.updated.v1`
- `worker.status_changed.v1`
- `worker.assignment_ended.v1`

Every event contains event ID, type, schema version, occurred time, organization ID, worker assignment ID, actor reference, correlation ID, and a minimized payload. Sensitive fields are excluded.

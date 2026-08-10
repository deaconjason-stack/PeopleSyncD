# Worker Permissions

- `worker.read.own` — read the caller's authorized assignment summary.
- `worker.read.direct_reports` — read permitted direct-report summaries.
- `worker.read.organization` — read organization directory summaries.
- `worker.create` — create assignments after person and organization validation.
- `worker.update` — update authorized non-confidential fields.
- `worker.end` — end an assignment with effective date and reason.
- `worker.read.history` — review historical assignments.

Default is deny. Board membership alone does not grant ordinary personnel-file access. Domonique 2.0 invokes the same permissions through narrow tools.

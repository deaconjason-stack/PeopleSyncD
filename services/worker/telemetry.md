# Worker Telemetry

Required metrics include request count, latency, error rate, authorization denials, tenant-mismatch denials, database latency, outbox backlog, event publication failures, and state-transition failures. Logs use structured correlation identifiers and exclude secrets and highly confidential fields. Traces must not capture request bodies containing workforce data by default.

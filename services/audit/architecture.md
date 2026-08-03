# Audit Architecture

Components: ingestion API, validator, append-only writer, integrity verifier, retention engine, legal-hold adapter, authorized query API, export service, event publisher, and monitoring.

Ingestion remains available under load without silently dropping evidence.

# PeopleSyncD Enterprise Platform Architecture

Clients connect through the API Platform to reusable platform services. Business applications consume those services through governed contracts. Data services and infrastructure remain independently managed but release-coordinated.

## Shared platform services

Identity, Organizations, Permissions, Licensing, Workflow, Notifications, Documents, Audit, Search, Reporting, Configuration, AI Engine, Integration Hub, Telemetry, Monitoring, Logging, Localization, and Accessibility.

## Data platform

PostgreSQL, Redis for approved ephemeral workloads, private object storage, search engine, event streaming, and a future governed data warehouse.

## Rule

Applications must not duplicate platform capabilities without an approved ADR describing the exception, trade-offs, migration, and removal plan.

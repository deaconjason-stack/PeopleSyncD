# PeopleSyncD Architecture

## M1.2.1 decisions

| Concern | Decision |
|---|---|
| Runtime | .NET 9 |
| Application shape | Clean Architecture modular monolith |
| Web | Next.js, React, TypeScript |
| Database | PostgreSQL |
| ORM | Entity Framework Core |
| Cache | Redis |
| Messaging | RabbitMQ planned after a concrete asynchronous workflow requires it |
| Identity | ASP.NET Core Identity persistence with JWT and OIDC-ready boundaries |
| API | REST first with OpenAPI |
| Observability | OpenTelemetry and Serilog |
| Local orchestration | .NET Aspire and Docker Compose |
| Deployment | Kubernetes starter manifests and Terraform modules |

## Dependency rule

`Api → Application → Domain → SharedKernel`

`Infrastructure → Application + Domain + SharedKernel`

Domain code has no EF Core, HTTP, ASP.NET Core, database, or vendor dependency. Infrastructure implements application-owned interfaces. API owns transport concerns only.

## Modular-monolith rule

M1 begins as a modular monolith to preserve transactional consistency and reduce operational complexity. A module becomes a separate service only when scale, isolation, ownership, or deployment evidence justifies the split.

## Security baseline

Tenant context, authorization, immutable audit evidence, secrets management, migration safety, dependency auditing, non-root containers, health checks, and private vulnerability reporting are release requirements—not optional hardening tasks.

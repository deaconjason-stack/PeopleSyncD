# PeopleSyncD Enterprise Platform

PeopleSyncD is an AI-powered enterprise workforce operating system. PeopleSyncD HR is the first commercial application; the shared platform and Domonique 2.0 are reusable products.

## Engineering status

Milestone M1.2.1 introduces an additive .NET 9 Clean Architecture solution, Next.js presentation shell, PostgreSQL persistence foundation, Docker development environment, Aspire orchestration, OpenAPI starter, Terraform skeleton, Kubernetes starter manifests, and automated quality gates.

The existing TypeScript Genesis implementation remains in place during the controlled transition. Neither implementation is a certified production release.

## Quick start

### .NET Aspire

```bash
dotnet workload restore PeopleSyncD.slnx
dotnet restore PeopleSyncD.slnx
dotnet run --project src/PeopleSyncD.AppHost
```

### Docker Compose

```bash
docker compose up --build
```

- Web: `http://localhost:3000`
- API: `http://localhost:8080`
- OpenAPI: `http://localhost:8080/openapi/v1.json`
- Health: `http://localhost:8080/health`

### Quality gate

```bash
dotnet format PeopleSyncD.slnx --verify-no-changes
dotnet build PeopleSyncD.slnx --configuration Release
dotnet test PeopleSyncD.slnx --configuration Release
cd src/PeopleSyncD.Web && npm install && npm run typecheck && npm run build
```

See [BUILD.md](BUILD.md), [ARCHITECTURE.md](ARCHITECTURE.md), and [CONTRIBUTING.md](CONTRIBUTING.md).

## Mandatory evidence chain

`Business Goal → Epic → Feature → Requirement → ADR → Contract → Code → Tests → Release Evidence → Customer Documentation → Feedback`

## Current board authority

The represented MediSyncD Technologies board consists only of Jason Henderson, Domonique Danielle Henderson, and Marietta Jessup.

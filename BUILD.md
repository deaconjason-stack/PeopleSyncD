# Build PeopleSyncD

## Prerequisites

- .NET SDK 9.
- Node.js 22 and npm 10.
- Docker with Compose v2.
- PostgreSQL 16 for direct persistence development.

## Restore and verify

```bash
dotnet restore PeopleSyncD.slnx -p:NuGetAudit=true -p:NuGetAuditMode=all
dotnet format PeopleSyncD.slnx --verify-no-changes --no-restore
dotnet build PeopleSyncD.slnx -c Release --no-restore
dotnet test PeopleSyncD.slnx -c Release --no-build --collect:"XPlat Code Coverage"
```

```bash
cd src/PeopleSyncD.Web
npm ci
npm run typecheck
npm run build
npm audit --audit-level=high
```

## Local orchestration

```bash
dotnet run --project src/PeopleSyncD.AppHost
```

Aspire starts PostgreSQL, Redis, the API, and the Next.js application.

```bash
docker compose up --build
```

The development API creates an ephemeral JWT signing key for the life of the process and initializes the development schema. Restarting the API invalidates previously issued development tokens.

## Production key requirement

Production startup requires `Jwt__SigningKey` from a protected secret provider. Do not store signing keys in appsettings files, Docker images, Kubernetes manifests, Terraform state, logs, or source control.

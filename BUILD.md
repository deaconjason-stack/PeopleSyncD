# Build PeopleSyncD

## Prerequisites

- .NET SDK 9
- Node.js 22 and npm 10
- Docker with Compose v2
- PostgreSQL 16 for direct local persistence work

## Restore and verify

```bash
dotnet restore PeopleSyncD.slnx -p:NuGetAudit=true -p:NuGetAuditMode=all
dotnet format PeopleSyncD.slnx --verify-no-changes --no-restore
dotnet build PeopleSyncD.slnx -c Release --no-restore
dotnet test PeopleSyncD.slnx -c Release --no-build --collect:"XPlat Code Coverage"
```

```bash
cd src/PeopleSyncD.Web
npm install
npm run typecheck
npm run build
npm audit --audit-level=high
```

## Local orchestration

`dotnet run --project src/PeopleSyncD.AppHost` starts PostgreSQL, Redis, the API, and the Next.js shell through Aspire.

`docker compose up --build` builds isolated API and web containers plus PostgreSQL and Redis.

Do not use local example passwords or unsigned images in customer environments.

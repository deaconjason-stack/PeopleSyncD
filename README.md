# PeopleSyncD Enterprise Platform

PeopleSyncD is an AI-powered enterprise workforce operating system. PeopleSyncD HR is the first commercial application; the shared platform and Domonique 2.0 are reusable products.

## Engineering status

Milestone M2.1 adds the first complete identity and tenant vertical slice on the .NET 9 and Next.js foundation.

The implemented flow supports owner and organization registration, password authentication, active organization-membership selection, short-lived tenant-scoped JWTs, role-derived permissions, tenant-bound organization access, and a working browser workspace.

The existing TypeScript Genesis implementation remains in place during the controlled transition. M2.1 is not a certified production release.

## Quick start

```bash
dotnet restore PeopleSyncD.slnx
dotnet run --project src/PeopleSyncD.AppHost
```

Or use containers:

```bash
docker compose up --build
```

- Web: `http://localhost:3000`
- Identity workspace: `http://localhost:3000/auth`
- API: `http://localhost:8080`
- Runtime OpenAPI: `http://localhost:8080/openapi/v1.json`
- Health: `http://localhost:8080/health`

See [BUILD.md](BUILD.md), [ARCHITECTURE.md](ARCHITECTURE.md), and [the M2.1 milestone record](docs/milestones/M2.1-identity-tenant.md).

## Mandatory evidence chain

`Business Goal → Epic → Feature → Requirement → ADR → Contract → Code → Tests → Release Evidence → Customer Documentation → Feedback`

## Current board authority

The represented MediSyncD Technologies board consists only of Jason Henderson, Domonique Danielle Henderson, and Marietta Jessup.

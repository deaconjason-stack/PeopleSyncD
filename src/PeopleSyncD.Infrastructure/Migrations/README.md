# EF Core Migrations

Migrations are generated only after domain, application, database, and security review.

```bash
dotnet ef migrations add <Name> \
  --project src/PeopleSyncD.Infrastructure \
  --startup-project src/PeopleSyncD.Api
```

Never apply an unreviewed migration to customer data. Production execution requires backup, rollback or forward-fix planning, tenant-isolation verification, and release evidence.

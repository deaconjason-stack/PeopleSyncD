# PeopleSyncD Web

Next.js presentation layer for the PeopleSyncD enterprise platform.

## M2.1 routes

- `/` summarizes the verified milestone capabilities.
- `/auth` provides tenant registration, login, organization selection, and current-session inspection.

## Local commands

```bash
npm ci
npm run typecheck
npm run build
npm run dev
```

Set `NEXT_PUBLIC_API_BASE_URL` to the browser-reachable PeopleSyncD API origin.

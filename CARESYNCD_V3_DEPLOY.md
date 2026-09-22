# CareSyncD v3 deployment branch

This branch is intentionally isolated from PeopleSyncD `main`.

## Canonical release

`releases/CareSyncD-v3-HOSPITAL-SHIFT.zip`

The Docker build extracts that exact tested release, runs the automated test suite and project integrity check, and only then starts the application.

## Railway

Deploy this branch with Railway using the root `Dockerfile` and `railway.json`.

Health check:

`/api/health`

The application listens on Railway's `PORT` environment variable and binds to `0.0.0.0`.

## Local Docker

```bash
docker build -t caresyncd-v3 .
docker run --rm -p 3000:3000 -e PORT=3000 caresyncd-v3
```

Open:

`http://localhost:3000`

## Safety boundary

CareSyncD v3 is an educational clinical simulation platform. It is not a medical device and is not intended to diagnose, monitor, or treat real patients.

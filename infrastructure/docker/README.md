# Docker Development Environment

`compose.yaml` provides local PostgreSQL, Redis, private object storage, and OpenTelemetry Collector dependencies for Genesis development.

## Start

```bash
cd infrastructure/docker
cp .env.example .env
docker compose --env-file .env up -d
```

## Stop

```bash
docker compose --env-file .env down
```

## Important boundary

This stack is for local development and integration testing. The example credentials are not production secrets. Production deployments require digest-pinned images, managed secrets, private networking, backups, monitoring, and approved certification evidence.

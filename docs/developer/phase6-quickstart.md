# Phase 6 Internal Alpha Quick Start

## Requirements

Node.js 22 or later and npm 10 or later.

## Windows

```powershell
powershell -ExecutionPolicy Bypass -File scripts/dev/start-genesis.ps1
```

## Linux or macOS

```bash
bash scripts/dev/start-genesis.sh
```

Open `http://127.0.0.1:5173` and select **Open Founder Workspace**. Development authentication is intentionally unavailable when `NODE_ENV=production`.

The application repository is currently in-memory. Restarting the API resets runtime-created records. PostgreSQL migrations are included and validated in CI, but the production PostgreSQL repository adapter is Phase 7 work.

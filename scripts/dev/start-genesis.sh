#!/usr/bin/env bash
set -euo pipefail

npm install
npm run build

cleanup() {
  kill "${API_PID:-}" "${WEB_PID:-}" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

PEOPLESYNCD_DEV_AUTH=true npm run dev:api &
API_PID=$!
npm run dev:web &
WEB_PID=$!

echo "PeopleSyncD API: http://127.0.0.1:8080"
echo "PeopleSyncD Web: http://127.0.0.1:5173"
wait "$API_PID" "$WEB_PID"

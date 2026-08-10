#!/usr/bin/env bash
set -euo pipefail

if [[ "${CODESPACES:-}" != "true" ]]; then
  echo "This launcher is intended for GitHub Codespaces."
  exit 1
fi

: "${CODESPACE_NAME:?CODESPACE_NAME is required}"
: "${GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN:?GitHub Codespaces forwarding domain is required}"

public_host="${CODESPACE_NAME}-3000.${GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN}"
public_url="https://${public_host}"

echo "Starting PeopleSyncD from GitHub at ${public_url}"
docker compose -f docker-compose.codespaces.yml up --detach --build

ready=false
for _ in $(seq 1 90); do
  if curl --fail --silent --show-error http://127.0.0.1:3000/ >/dev/null 2>&1; then
    ready=true
    break
  fi
  sleep 2
done

if [[ "${ready}" != "true" ]]; then
  echo "PeopleSyncD did not become ready."
  docker compose -f docker-compose.codespaces.yml ps
  exit 1
fi

if command -v gh >/dev/null 2>&1; then
  if gh codespace ports visibility 3000:public -c "${CODESPACE_NAME}" >/dev/null 2>&1; then
    echo "Port 3000 is public."
  else
    echo "GitHub did not allow automatic public-port visibility. Use the Ports panel to set port 3000 to Public."
  fi
else
  echo "GitHub CLI is unavailable. Use the Ports panel to set port 3000 to Public."
fi

printf '\nPeopleSyncD live URL: %s\n' "${public_url}"
printf 'Demo boundary: development environment only; do not enter real confidential or regulated data.\n'

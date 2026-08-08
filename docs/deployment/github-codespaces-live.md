# GitHub Codespaces Live Demo

## Purpose

This configuration runs the real PeopleSyncD engineering stack directly from a GitHub Codespace for demonstration and review. It is not a production deployment.

## Runtime shape

A Codespace starts Docker Compose with:

- PostgreSQL 16 on the private container network;
- Redis on the private container network;
- the PeopleSyncD .NET API on the private container network;
- the PeopleSyncD Next.js web application on the private container network; and
- a Caddy gateway exposed only through forwarded port 3000.

The gateway preserves a single browser origin. `/api/*` is routed to the .NET API and all other requests are routed to the Next.js application. PostgreSQL, Redis, and the API container are not separately published to the internet.

## WebAuthn

The relying-party ID and accepted origin are derived at Codespace startup from `CODESPACE_NAME` and `GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN`. This allows passkey ceremonies to use the HTTPS `app.github.dev` forwarded origin instead of the localhost development origin.

## Start behavior

The dev container forwards port 3000 and runs `scripts/codespaces/start-live.sh` after startup. The launcher builds and starts the Compose stack, waits for the gateway to answer, and asks GitHub CLI to make port 3000 public when account and organization policy allow it.

If GitHub policy prevents automatic public visibility, use the Codespaces Ports panel and set port 3000 to Public.

## Security boundary

This is a development demonstration environment. It uses development database credentials and a development signing-key path. Do not enter real employee records, regulated data, production secrets, customer credentials, or other confidential data.

A public forwarded Codespaces port is temporary. GitHub may return the port to private visibility after a restart, and a Codespace can stop after inactivity. This configuration attempts to restore public visibility each time the Codespace starts.

## Production boundary

A persistent production PeopleSyncD deployment still requires a durable application/container host, managed PostgreSQL/Redis, production secrets and keys, hardened browser session custody, production WebAuthn RP/origin configuration, operational monitoring, backup/restore, independent security evidence, and signed release controls.

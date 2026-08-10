# Secrets and Key Management

Production secrets must be created outside the repository and injected through an approved secret manager, sealed deployment mechanism, or protected CI environment.

Required classes include database credentials, object-storage credentials, session-signing material, application encryption keys, integration credentials, AI-provider credentials, release-signing keys, and Windows code-signing certificates.

Never paste a private key, certificate password, recovery code, access token, or production secret into issues, pull requests, logs, support bundles, prompts, or configuration files.

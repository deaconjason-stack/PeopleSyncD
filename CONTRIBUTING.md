# Contributing to PeopleSyncD

## Governance-first workflow

1. Link the change to a PEP, ADR, RFC, or requirement identifier.
2. Create a focused branch.
3. Update contracts, schemas, tests, traceability, and documentation together.
4. Open a pull request; do not bypass review for governance, security, architecture, database, API, or release changes.
5. Ensure CI passes before merge.

## Required evidence

A change is complete only when applicable requirements, implementation, API or event contracts, database effects, authorization rules, audit behavior, tests, migration notes, and release evidence agree.

## Commit guidance

Use clear conventional prefixes such as `feat:`, `fix:`, `docs:`, `security:`, `test:`, `refactor:`, and `chore:`.

## Safety

Never commit secrets or real workforce data. Use fictional or anonymized test records. Domonique 2.0 features must use narrowly scoped tools, tenant isolation, permission enforcement, source grounding, human approval for high-impact actions, and complete audit logging.

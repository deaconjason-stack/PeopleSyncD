# Contributing to PeopleSyncD

## Repository-first workflow

1. Link work to an epic, requirement, ADR, issue, or release gate.
2. Create one focused branch and one reviewable logical change.
3. Update code, tests, contracts, infrastructure, security effects, documentation, and traceability together.
4. Run the complete local quality gate.
5. Open a pull request and resolve review and CI findings before merge.

## Required quality gates

- `dotnet format` reports no changes.
- .NET and Next.js builds succeed.
- Unit and integration tests pass.
- Coverage evidence is generated and must not regress without approved justification.
- NuGet and npm audits contain no unresolved high-severity findings.
- OpenAPI and database changes are reviewed with compatibility and migration notes.
- Documentation and operational guidance match the implementation.

## Coding standards

Use nullable reference types, file-scoped namespaces, deterministic builds, central package versions, asynchronous I/O, cancellation tokens, stable error codes, XML documentation for public APIs, and Microsoft naming guidance.

Never commit secrets, private keys, real workforce data, customer documents, raw production prompts, or unrestricted support bundles.

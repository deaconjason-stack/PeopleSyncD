# HR Domain Test Strategy

## Test families

- Unit tests for domain rules and state transitions
- OpenAPI and AsyncAPI contract tests
- PostgreSQL migration and row-level-security tests
- Cross-tenant access tests
- Role, relationship, and explicit-assignment authorization tests
- Audit and immutable-history tests
- Document and notification privacy tests
- Accessibility and role-aware UI acceptance tests
- Domonique 2.0 authorization, source-grounding, refusal, and human-approval tests
- Backup, restore, rollback, and resilience tests

Every automated test receives a permanent `PSD-TEST-*` identifier before release certification.

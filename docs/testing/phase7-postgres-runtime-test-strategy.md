# Phase 7 PostgreSQL Runtime Test Strategy

## Objective

Verify that the API's production persistence adapter behaves correctly under the same PostgreSQL security controls expected in deployment.

## Automated coverage

The Software Foundation workflow shall:

1. start PostgreSQL 16;
2. install dependencies;
3. apply every ordered SQL migration;
4. verify seed records;
5. verify forced row-level security;
6. run strict TypeScript checks;
7. run unit, contract, and live PostgreSQL tests;
8. compile API, web, and desktop applications;
9. generate an SBOM and checksums; and
10. upload commit-linked evidence.

## Tenant-isolation test

The live integration test creates a record in a secondary organization through the constrained repository and confirms:

- the secondary organization can retrieve the record; and
- the Genesis organization cannot retrieve that record.

The test database is disposable. Test-created Person and Organization records are removed after execution.

## Remaining coverage

This strategy does not yet prove production identity federation, MFA, database failover, backup restoration, disaster recovery, performance budgets, accessibility, or penetration resistance. Those require separate evidence before certification.

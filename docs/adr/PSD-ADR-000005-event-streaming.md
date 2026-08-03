---
id: PSD-ADR-000005
title: Evaluate Kafka-Compatible Event Streaming for Enterprise Deployments
version: 0.1.0
status: Proposed
classification: Internal
owner: Enterprise Architecture Office
review_cycle: Until Resolved
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-130
  - PSD-ADR-000010
---

# Evaluate Kafka-Compatible Event Streaming for Enterprise Deployments

## Context

PeopleSyncD requires versioned domain events, durable delivery, consumer groups, retry, replay, dead-letter handling, and enterprise observability. Smaller deployments may not justify Kafka operational cost.

## Proposed decision

Define events independently through AsyncAPI. Evaluate Kafka-compatible streaming for cloud and large enterprise deployments while permitting a simpler compatible broker for development and smaller installations.

## Required evidence

The final decision must compare security, ordering, tenant isolation, operations, cost, restricted-environment support, backup, recovery, and migration.

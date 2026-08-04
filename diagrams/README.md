# Diagrams

This directory contains version-controlled diagram sources and rendering guidance for the PeopleSyncD Enterprise Master Blueprint.

## Standards

- Prefer text-based, reviewable sources such as Mermaid where practical.
- Every diagram must identify its scope, owner, source specification, and last verified version.
- Diagrams are explanatory views; governed specifications and contracts remain authoritative.
- Sensitive infrastructure, customer, credential, network, and personal details must not appear in public or broadly shared diagrams.
- Generated image files must be reproducible from committed sources whenever possible.

## Required diagram families

- Layered enterprise architecture
- Company and product operating model
- Product and application portfolio
- System context and trust boundaries
- Service and dependency topology
- Identity, session, tenant, and permission flows
- Domonique 2.0 conversation, retrieval, memory, tool, and approval flows
- Core HR domain and ERD
- Event and integration topology
- Deployment profiles and network boundaries
- CI/CD, evidence, and release flow
- Backup, restoration, incident, and disaster-recovery flows

Each diagram should link back to `PEOPLESYNCD-ENTERPRISE-MASTER-BLUEPRINT-V1.0.md`, the relevant `PSD-PEP-*`, and any governing `PSD-ADR-*`.

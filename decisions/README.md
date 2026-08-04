# Decisions

This directory is the navigation layer for PeopleSyncD architecture and product decisions. The controlled records remain in:

- `docs/adr/` for Architecture Decision Records
- `docs/rfc/` for Requests for Comments
- `docs/governance/` for executive, board, and program decisions

## Decision rules

- Significant architectural change requires an ADR or RFC before implementation.
- Decisions identify context, options, trade-offs, security and privacy impact, data impact, operational impact, migration, rollback, and consequences.
- Accepted decisions are immutable records; later changes supersede rather than rewrite history.
- Experimental choices remain RFCs until approved.
- Emergency decisions require a documented exception and retrospective review.
- Every accepted decision links to affected requirements, specifications, contracts, code, tests, release evidence, and customer documentation.

## Index expectations

The generated decision index must show identifier, title, status, owner, approval date, superseded records, affected domains, implementation status, and verification evidence.

Decision status does not by itself prove implementation or release. Traceability must demonstrate the complete lifecycle.

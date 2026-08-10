---
id: PSD-PEP-505
title: Software Supply Chain and Release Security
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: DevSecOps Office
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-0040
  - PSD-PEP-120
  - PSD-PEP-2090
---

# Software Supply Chain and Release Security

## Purpose

Protect source, dependencies, build systems, artifacts, containers, installers, infrastructure packages, and release evidence.

## Controls

- Protected branches and reviewed changes
- Dependency and license inventory
- Secret and static analysis
- Reproducible or explainable builds
- Isolated build runners and least-privileged workflow permissions
- Software bill of materials generation
- Checksums and detached signatures
- Digest-pinned production images
- Provenance tied to source commit and workflow run
- Vulnerability triage and documented risk acceptance
- Owner-controlled Windows code-signing certificate for final desktop installers

Unsigned or unverifiable production artifacts cannot be certified.

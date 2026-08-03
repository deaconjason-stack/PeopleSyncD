# Traceability Matrix

The machine-readable source is `requirements.csv` and the approved requirement catalog is `docs/requirements/registry.yaml`.

## Current status

Genesis contains approved architecture requirements and draft service requirements. No requirement is represented as implemented until code, automated tests, and release evidence are linked and verified.

## Required evidence chain

`Requirement → Specification → Architecture Decision → Contract → Implementation → Automated Tests → Certification`

## Status meanings

- `planned` means approved but not yet implemented.
- `draft` means recorded for review and not yet promoted to the master requirement registry.
- `implemented` requires code and automated test references.
- `verified` requires passing evidence tied to a commit.
- `released` requires an approved release manifest and certification decision.

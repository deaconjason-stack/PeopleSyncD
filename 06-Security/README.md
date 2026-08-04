# 06 — Security

**Domain ID:** PSD-DOM-SEC-006  
**Accountable function:** Security, Privacy, Risk, and Compliance  
**Purpose:** Apply zero-trust security, identity assurance, privacy, threat management, compliance evidence, and customer trust controls across every layer.

## Canonical sources

- `SECURITY.md`
- `docs/security/`
- `docs/requirements/security/`
- `docs/specifications/PSD-PEP-120-security-architecture.md`
- `docs/specifications/PSD-PEP-140-identity-architecture.md`
- `docs/specifications/PSD-PEP-407-ai-security-evaluation.md`
- `.github/workflows/security.yml`
- `tests/security/`

## Required artifacts

- Zero-trust architecture and trust boundaries
- IAM, workload identity, privileged-access, and session controls
- Threat models and abuse cases
- Secure-development, dependency, secret, and supply-chain controls
- Privacy classification, purpose limitation, consent, retention, and disclosure controls
- Security operations, detection, response, vulnerability, and incident procedures
- Compliance mappings and evidence indexes
- Trust Center content grounded in verified controls
- Independent testing and remediation records

## Rules

- Authenticate every actor and workload; authorize every protected action.
- Require current tenant, membership, permission, policy, and session authority.
- Default deny when context or evidence is missing.
- Separate migration, runtime, support, and administrative identities.
- Secrets never belong in source control.
- High-impact actions require human approval and, where defined, separation of duties.
- The last active Founder cannot be silently suspended, ended, or stripped of management authority.
- Compliance claims must identify scope, evidence, assessor, date, and limitations.

## Completion gate

A release cannot be approved until required threat models, security tests, dependency and secret scans, vulnerability decisions, privacy review, incident readiness, and deployment-specific controls are complete or explicitly risk accepted.

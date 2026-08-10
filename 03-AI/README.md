# 03 — AI

**Domain ID:** PSD-DOM-AI-003  
**Accountable function:** AI Engineering and AI Governance  
**Purpose:** Govern Domonique 2.0, models, knowledge, memory, prompts, tools, evaluations, safety, and human control.

## Canonical sources

- `docs/specifications/PSD-PEP-150-ai-governance.md`
- `docs/specifications/PSD-PEP-400-domonique-ai-platform.md`
- `docs/specifications/PSD-PEP-401-conversation-prompt-engine.md`
- `docs/specifications/PSD-PEP-402-retrieval-grounding-policy.md`
- `docs/specifications/PSD-PEP-403-tools-approvals.md`
- `docs/specifications/PSD-PEP-404-memory-vault.md`
- `docs/specifications/PSD-PEP-405-assistant-modes.md`
- `docs/specifications/PSD-PEP-406-voice-interface.md`
- `docs/specifications/PSD-PEP-407-ai-security-evaluation.md`
- `docs/api/ai.yaml`
- `packages/ai/`

## Required artifacts

- Domonique 2.0 reference architecture and Agent OS boundaries
- Model registry, routing, version, cost, safety, and retirement records
- Knowledge graph, retrieval, grounding, and provenance standards
- Memory classification, consent, retention, correction, and deletion controls
- Tool registry with schemas, permissions, risk, approvals, evidence, and rollback
- Prompt, response, evaluation, red-team, and regression standards
- Human-approval policy for high-impact actions
- AI incident, drift, quality, and abuse monitoring

## Rules

- Domonique 2.0 is tenant-aware, permission-aware, source-grounded, auditable, and human-controlled.
- AI cannot silently perform high-impact employment, legal, financial, credentialing, licensing, disciplinary, disclosure, or governance actions.
- The underlying business platform must remain operable when AI is disabled.
- Model output is untrusted until policy, authorization, schema, and approval checks pass.
- Prompts, retrieved sources, tool calls, approvals, and outcomes require privacy-safe traceability.

## Completion gate

An AI capability is releasable only when model and data lineage, tool permissions, approval behavior, failure behavior, evaluation thresholds, safety tests, monitoring, rollback, and user disclosure are verified.

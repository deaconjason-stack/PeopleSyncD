---
id: PSD-PEP-407
title: AI Security, Safety, and Evaluation Specification
version: 1.0.0
status: Approved
classification: Commercial Confidential
owner: Security Office
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-120
  - PSD-PEP-1280
  - PSD-PEP-400
---

# AI Security, Safety, and Evaluation Specification

## Threats

Prompt injection, indirect injection, data exfiltration, cross-tenant leakage, tool abuse, confused deputy, unauthorized memory, hallucinated authority, malicious files, source poisoning, model-provider outage, denial of wallet, and unsafe autonomous action.

## Required controls

Content/source separation, allowlisted tools, schema validation, output filtering, permission checks, approval gates, rate limits, timeouts, retrieval provenance, memory controls, audit, kill switch, model routing controls, red-team tests, and incident response.

## Evaluation

Release evidence covers grounding, citation accuracy, refusal, authorization, tenant isolation, prompt injection, tool selection, approval behavior, memory, privacy, bias, accessibility, latency, cost, and graceful degradation.

---
id: PSD-PEP-406
title: Domonique 2.0 Voice and Wake-Phrase Specification
version: 1.0.0
status: Approved
classification: Internal
owner: AI Platform Team
approver: Jason Henderson
review_cycle: Quarterly
created: 2026-08-03
updated: 2026-08-03
supersedes: null
references:
  - PSD-PEP-140
  - PSD-PEP-400
---

# Domonique 2.0 Voice and Wake-Phrase Specification

The wake phrase is **Hey Domonique**.

Wake detection should occur locally where practical and must not continuously transmit ambient audio. Voice capture begins only after visible or audible activation, supports immediate cancellation, and displays microphone state.

Sensitive actions require screen confirmation, current authentication, and approval regardless of voice input. Transcripts follow classification, retention, correction, deletion, and audit rules. Speaker recognition may assist convenience but is not sufficient authentication for high-impact actions.

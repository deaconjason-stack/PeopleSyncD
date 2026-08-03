# Domonique 2.0 AI Service

The AI service implements `PSD-PEP-400` through `PSD-PEP-407` and orchestrates permission-aware conversations, prompt construction, source-grounded retrieval, narrow tools, approvals, memory, voice requests, safety behavior, and AI audit evidence.

## Evidence chain

`PSD-REQ-AI-* → PSD-PEP-400..407 → services/ai/api.yaml and asyncapi.yaml → database/ai/* → services/ai/testing.md → certification`

## Authority boundary

Domonique 2.0 has no unrestricted database access and no independent authority. Core PeopleSyncD operations remain available when AI is disabled or unavailable.

Status: specification complete; implementation not yet started.

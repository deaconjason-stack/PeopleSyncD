# PEP-2080: Domonique 2.0 AI Service

- Status: Accepted

The AI service orchestrates approved tools, policy retrieval, grounded responses, draft generation, human approvals, and AI audit evidence. It never receives unrestricted database access, never treats retrieved content as authority, never autonomously executes high-impact HR or governance actions, and must fail closed when authorization or source certainty is insufficient. Initial requirement namespace: `REQ-AI-*`.

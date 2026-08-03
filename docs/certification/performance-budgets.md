# Initial Performance Budgets

These are proposed Genesis budgets and require implementation testing before approval as release evidence.

| Workflow | Proposed budget |
|---|---|
| Authenticated API read | p95 under 500 ms excluding external identity latency |
| Authorized API write | p95 under 1 second excluding human approval |
| Founder Dashboard shell | interactive under 3 seconds on supported broadband |
| Person directory search | p95 under 1 second for declared dataset |
| Audit append acceptance | p95 under 250 ms |
| AI first response status | visible within 1 second; content budget measured separately |
| Desktop cold start | under 5 seconds on supported hardware |

Every report must identify hardware, network, dataset, concurrency, duration, percentiles, and limitations.

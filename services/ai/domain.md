# Domonique 2.0 AI Domain

## Entities

Conversation, Message, Prompt Template, Source Reference, Retrieval Result, Tool Definition, Tool Invocation, Action Request, Approval, Memory Record, Assistant Mode, Model Route, Safety Decision, Evaluation Result, and AI Audit Reference.

## Action lifecycle

`Drafted → Policy Checked → Approval Required or Ready → Approved → Revalidated → Executed → Confirmed`

Alternate terminal states are Refused, Canceled, Expired, Failed, and Rolled Back.

## Invariants

- Every record has tenant and actor context.
- Tool permissions are rechecked at invocation and execution.
- Retrieved content cannot modify policy.
- High-impact actions cannot skip approval.
- Memory access is reauthorized at read time.
- Sources and generated statements remain distinguishable.

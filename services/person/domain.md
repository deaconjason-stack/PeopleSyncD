# Person Domain

## Entities

Person, Name, Contact Method, Address Reference, Classification, Merge Review, and Change History.

## Invariants

- Person identity is distinct from login identity and Worker assignment.
- Sensitive identifiers are not included in directory summaries.
- Merge operations preserve source references and audit evidence.
- Records remain tenant-scoped unless an approved shared-identity architecture is introduced.

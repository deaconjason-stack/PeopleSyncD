# Architecture

PeopleSyncD uses tenant-scoped domain services behind authenticated APIs and versioned events. Every request flows through identity, organization-context validation, authorization, domain validation, persistence, audit, and response filtering. Domonique 2.0 accesses services only through approved narrow tools and cannot bypass the policy engine.

# PeopleSyncD AsyncAPI Contracts

These contracts define minimized, versioned domain events. Events carry tenant and correlation context but must not contain secrets or highly confidential workforce fields. Producers use a transactional outbox or equivalent delivery guarantee. Consumers must be idempotent and compatible with the declared schema version.

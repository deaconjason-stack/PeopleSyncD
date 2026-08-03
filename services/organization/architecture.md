# Organization Architecture

Components: organization API, hierarchy engine, membership manager, context issuer, settings manager, event outbox, and audit adapter.

The service validates tenant context before downstream use and publishes minimized hierarchy and membership events.

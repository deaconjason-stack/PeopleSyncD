# Notification Architecture

Components: request API, recipient and permission resolver, template renderer, preference evaluator, channel adapters, retry scheduler, dead-letter store, delivery-status tracker, event outbox, and audit adapter.

Authorization is checked before rendering sensitive content and again when following related-record links.

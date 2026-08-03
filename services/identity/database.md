# Identity Database Contract

Owned objects: user accounts, credential references, authenticators, sessions, identity providers, recovery artifacts, and authentication-event outbox.

Passwords and recovery secrets are never stored in plaintext. Session and credential indexes support revocation and replay detection. Schema migrations preserve account and audit history.

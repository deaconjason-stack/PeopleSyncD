# Identity Requirements

- `PSD-REQ-IDENTITY-000001` Strong authentication and session control.
- `PSD-REQ-IDENTITY-000002` Federated identity lifecycle.

Additional service rules:

- Privileged roles require MFA.
- Refresh credentials rotate and replay is denied.
- Account disablement revokes active sessions.
- Recovery is rate-limited, auditable, and cannot bypass MFA policy.
- Authentication never substitutes for organization membership or authorization.

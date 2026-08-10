# PEP-1210: Authentication

- Status: Accepted

Authentication verifies user identity independently from person and worker records. Passwords use approved adaptive hashing. Supported methods may include passkeys, MFA, security keys, and enterprise SSO. Login, recovery, device trust, lockout, and account-status decisions are audited. Authentication alone never grants tenant or record access.

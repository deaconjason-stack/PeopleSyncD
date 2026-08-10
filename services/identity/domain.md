# Identity Domain

Entities: User Account, Credential, Authenticator, Session, Identity Provider, Recovery Artifact, Authentication Event.

Account states: `Invited → Active → Locked or Suspended → Disabled → Archived`.

Session states: `Issued → Active → Rotated → Revoked or Expired`.

Invariants include unique normalized login identifiers, encrypted or hashed secrets, one-time recovery artifacts, explicit identity-provider trust, and auditable state changes.

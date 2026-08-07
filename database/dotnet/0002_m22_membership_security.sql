BEGIN;

CREATE TABLE IF NOT EXISTS organization_invitations (
    "Id" uuid PRIMARY KEY,
    "OrganizationId" uuid NOT NULL REFERENCES organizations ("Id") ON DELETE CASCADE,
    "InvitedByUserId" uuid NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    "Email" varchar(320) NOT NULL,
    "DisplayName" varchar(200) NOT NULL,
    "Role" varchar(32) NOT NULL,
    "TokenHash" varchar(64) NOT NULL,
    "Status" varchar(32) NOT NULL,
    "CreatedAt" timestamptz NOT NULL,
    "ExpiresAt" timestamptz NOT NULL,
    "AcceptedAt" timestamptz,
    "RevokedAt" timestamptz,
    CONSTRAINT ck_invitation_role CHECK ("Role" IN ('Administrator','Manager','Member','Auditor')),
    CONSTRAINT ck_invitation_status CHECK ("Status" IN ('Pending','Accepted','Revoked','Expired')),
    CONSTRAINT ck_invitation_expiry CHECK ("ExpiresAt" > "CreatedAt")
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_invitation_token_hash
    ON organization_invitations ("TokenHash");
CREATE INDEX IF NOT EXISTS ix_invitation_org_email_status
    ON organization_invitations ("OrganizationId", "Email", "Status");

CREATE TABLE IF NOT EXISTS refresh_sessions (
    "Id" uuid PRIMARY KEY,
    "FamilyId" uuid NOT NULL,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    "OrganizationId" uuid,
    "MembershipId" uuid,
    "ParentSessionId" uuid,
    "TokenHash" varchar(64) NOT NULL,
    "CreatedAt" timestamptz NOT NULL,
    "ExpiresAt" timestamptz NOT NULL,
    "UsedAt" timestamptz,
    "RevokedAt" timestamptz,
    "RevokeReason" varchar(128),
    CONSTRAINT fk_refresh_organization FOREIGN KEY ("OrganizationId") REFERENCES organizations ("Id") ON DELETE CASCADE,
    CONSTRAINT fk_refresh_membership FOREIGN KEY ("MembershipId") REFERENCES organization_memberships ("Id") ON DELETE CASCADE,
    CONSTRAINT fk_refresh_parent FOREIGN KEY ("ParentSessionId") REFERENCES refresh_sessions ("Id") ON DELETE SET NULL,
    CONSTRAINT ck_refresh_expiry CHECK ("ExpiresAt" > "CreatedAt"),
    CONSTRAINT ck_refresh_tenant_pair CHECK (("OrganizationId" IS NULL) = ("MembershipId" IS NULL))
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_refresh_token_hash
    ON refresh_sessions ("TokenHash");
CREATE UNIQUE INDEX IF NOT EXISTS ux_refresh_parent_session
    ON refresh_sessions ("ParentSessionId")
    WHERE "ParentSessionId" IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_refresh_family_revoked
    ON refresh_sessions ("FamilyId", "RevokedAt");
CREATE INDEX IF NOT EXISTS ix_refresh_membership_revoked
    ON refresh_sessions ("MembershipId", "RevokedAt");

CREATE TABLE IF NOT EXISTS security_audit_records (
    "Id" uuid PRIMARY KEY,
    "EventType" varchar(128) NOT NULL,
    "ActorUserId" uuid,
    "OrganizationId" uuid,
    "TargetType" varchar(64) NOT NULL,
    "TargetId" varchar(128) NOT NULL,
    "OccurredAt" timestamptz NOT NULL,
    "MetadataJson" jsonb NOT NULL DEFAULT '{}'::jsonb
);

CREATE INDEX IF NOT EXISTS ix_security_audit_organization_time
    ON security_audit_records ("OrganizationId", "OccurredAt");
CREATE INDEX IF NOT EXISTS ix_security_audit_actor_time
    ON security_audit_records ("ActorUserId", "OccurredAt");

COMMIT;

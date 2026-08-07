BEGIN;

ALTER TABLE refresh_sessions
    ADD COLUMN IF NOT EXISTS "LastSeenAt" timestamptz,
    ADD COLUMN IF NOT EXISTS "AssuranceLevel" varchar(16) NOT NULL DEFAULT 'pwd',
    ADD COLUMN IF NOT EXISTS "DeviceLabel" varchar(256);

UPDATE refresh_sessions
SET "LastSeenAt" = "CreatedAt"
WHERE "LastSeenAt" IS NULL;

ALTER TABLE refresh_sessions
    ALTER COLUMN "LastSeenAt" SET NOT NULL;

ALTER TABLE refresh_sessions
    DROP CONSTRAINT IF EXISTS ck_refresh_assurance;
ALTER TABLE refresh_sessions
    ADD CONSTRAINT ck_refresh_assurance CHECK ("AssuranceLevel" IN ('pwd','mfa'));

CREATE INDEX IF NOT EXISTS ix_refresh_user_family_active
    ON refresh_sessions ("UserId", "FamilyId", "RevokedAt");

CREATE TABLE IF NOT EXISTS mfa_challenges (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    "Purpose" varchar(32) NOT NULL,
    "TokenHash" varchar(64) NOT NULL,
    "OrganizationId" uuid,
    "MembershipId" uuid,
    "CreatedAt" timestamptz NOT NULL,
    "ExpiresAt" timestamptz NOT NULL,
    "CompletedAt" timestamptz,
    "FailedAttempts" integer NOT NULL DEFAULT 0,
    CONSTRAINT fk_mfa_challenge_organization FOREIGN KEY ("OrganizationId") REFERENCES organizations ("Id") ON DELETE SET NULL,
    CONSTRAINT fk_mfa_challenge_membership FOREIGN KEY ("MembershipId") REFERENCES organization_memberships ("Id") ON DELETE SET NULL,
    CONSTRAINT ck_mfa_challenge_purpose CHECK ("Purpose" IN ('login','step_up')),
    CONSTRAINT ck_mfa_challenge_context CHECK (("OrganizationId" IS NULL) = ("MembershipId" IS NULL)),
    CONSTRAINT ck_mfa_challenge_expiry CHECK ("ExpiresAt" > "CreatedAt"),
    CONSTRAINT ck_mfa_challenge_attempts CHECK ("FailedAttempts" BETWEEN 0 AND 5)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_mfa_challenge_token_hash
    ON mfa_challenges ("TokenHash");
CREATE INDEX IF NOT EXISTS ix_mfa_challenge_user_expiry
    ON mfa_challenges ("UserId", "ExpiresAt");

CREATE TABLE IF NOT EXISTS mfa_recovery_codes (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    "BatchId" uuid NOT NULL,
    "CodeHash" varchar(64) NOT NULL,
    "CreatedAt" timestamptz NOT NULL,
    "UsedAt" timestamptz,
    "RevokedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_mfa_recovery_code_hash
    ON mfa_recovery_codes ("CodeHash");
CREATE INDEX IF NOT EXISTS ix_mfa_recovery_user_active
    ON mfa_recovery_codes ("UserId", "RevokedAt", "UsedAt");

COMMIT;

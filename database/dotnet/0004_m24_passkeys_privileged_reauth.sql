BEGIN;

ALTER TABLE refresh_sessions
    ADD COLUMN IF NOT EXISTS "AuthenticatedAt" timestamptz;

UPDATE refresh_sessions
SET "AuthenticatedAt" = "CreatedAt"
WHERE "AuthenticatedAt" IS NULL;

ALTER TABLE refresh_sessions
    ALTER COLUMN "AuthenticatedAt" SET NOT NULL;

ALTER TABLE refresh_sessions
    ADD COLUMN IF NOT EXISTS "AuthenticationMethod" character varying(32);

UPDATE refresh_sessions
SET "AuthenticationMethod" = CASE
    WHEN "AssuranceLevel" = 'mfa' THEN 'totp'
    ELSE 'pwd'
END
WHERE "AuthenticationMethod" IS NULL OR btrim("AuthenticationMethod") = '';

ALTER TABLE refresh_sessions
    ALTER COLUMN "AuthenticationMethod" SET NOT NULL;

CREATE TABLE IF NOT EXISTS passkey_credentials (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "CredentialId" character varying(1024) NOT NULL,
    "PublicKey" bytea NOT NULL,
    "UserHandle" bytea NOT NULL,
    "SignatureCounter" bigint NOT NULL,
    "DisplayName" character varying(200) NOT NULL,
    "Transports" character varying(256),
    "BackupEligible" boolean NOT NULL,
    "BackedUp" boolean NOT NULL,
    "AaGuid" uuid NOT NULL,
    "CreatedAt" timestamptz NOT NULL,
    "LastUsedAt" timestamptz,
    "RevokedAt" timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_passkey_credential_id
    ON passkey_credentials ("CredentialId");
CREATE INDEX IF NOT EXISTS ix_passkey_user_active
    ON passkey_credentials ("UserId", "RevokedAt");

CREATE TABLE IF NOT EXISTS passkey_ceremonies (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    "Purpose" character varying(32) NOT NULL,
    "OptionsJson" text NOT NULL,
    "OrganizationId" uuid,
    "MembershipId" uuid,
    "CreatedAt" timestamptz NOT NULL,
    "ExpiresAt" timestamptz NOT NULL,
    "CompletedAt" timestamptz
);

CREATE INDEX IF NOT EXISTS ix_passkey_ceremony_user_purpose_expiry
    ON passkey_ceremonies ("UserId", "Purpose", "ExpiresAt");

COMMIT;

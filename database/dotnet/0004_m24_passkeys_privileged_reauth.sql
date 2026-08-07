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

COMMIT;

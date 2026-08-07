BEGIN;

CREATE TABLE IF NOT EXISTS organizations (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(200) NOT NULL,
    "Slug" varchar(80) NOT NULL,
    "CreatedAt" timestamptz NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_organizations_slug
    ON organizations ("Slug");

CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" uuid PRIMARY KEY,
    "UserName" varchar(256),
    "NormalizedUserName" varchar(256),
    "Email" varchar(256),
    "NormalizedEmail" varchar(256),
    "EmailConfirmed" boolean NOT NULL DEFAULT false,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL DEFAULT false,
    "TwoFactorEnabled" boolean NOT NULL DEFAULT false,
    "LockoutEnd" timestamptz,
    "LockoutEnabled" boolean NOT NULL DEFAULT true,
    "AccessFailedCount" integer NOT NULL DEFAULT 0,
    "PersonId" uuid,
    "DisplayName" varchar(200) NOT NULL DEFAULT '',
    "IsActive" boolean NOT NULL DEFAULT true
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_aspnetusers_normalized_email
    ON "AspNetUsers" ("NormalizedEmail")
    WHERE "NormalizedEmail" IS NOT NULL;

CREATE TABLE IF NOT EXISTS organization_memberships (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    "OrganizationId" uuid NOT NULL REFERENCES organizations ("Id") ON DELETE CASCADE,
    "Role" varchar(32) NOT NULL,
    "Status" varchar(32) NOT NULL,
    "CreatedAt" timestamptz NOT NULL,
    "UpdatedAt" timestamptz NOT NULL,
    CONSTRAINT ck_membership_role CHECK ("Role" IN ('Owner','Administrator','Manager','Member','Auditor')),
    CONSTRAINT ck_membership_status CHECK ("Status" IN ('Active','Suspended','Revoked'))
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_membership_user_organization
    ON organization_memberships ("UserId", "OrganizationId");
CREATE INDEX IF NOT EXISTS ix_membership_organization_status
    ON organization_memberships ("OrganizationId", "Status");

COMMIT;

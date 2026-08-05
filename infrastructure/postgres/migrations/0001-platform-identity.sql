BEGIN;

CREATE TABLE IF NOT EXISTS organizations (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(200) NOT NULL,
    "Slug" varchar(80) NOT NULL,
    "CreatedAt" timestamptz NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_organizations_Slug" ON organizations ("Slug");

ALTER TABLE IF EXISTS "AspNetUsers"
    ADD COLUMN IF NOT EXISTS "DisplayName" varchar(200) NOT NULL DEFAULT '',
    ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT true,
    ADD COLUMN IF NOT EXISTS "PersonId" uuid NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AspNetUsers_NormalizedEmail_Unique"
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
    CONSTRAINT "CK_organization_memberships_role"
        CHECK ("Role" IN ('Owner', 'Administrator', 'Manager', 'Member', 'Auditor')),
    CONSTRAINT "CK_organization_memberships_status"
        CHECK ("Status" IN ('Active', 'Suspended', 'Revoked'))
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_organization_memberships_UserId_OrganizationId"
    ON organization_memberships ("UserId", "OrganizationId");
CREATE INDEX IF NOT EXISTS "IX_organization_memberships_OrganizationId_Status"
    ON organization_memberships ("OrganizationId", "Status");

COMMIT;

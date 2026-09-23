BEGIN;

CREATE TABLE IF NOT EXISTS employees (
    "Id" uuid PRIMARY KEY,
    "OrganizationId" uuid NOT NULL REFERENCES organizations ("Id") ON DELETE CASCADE,
    "EmployeeNumber" varchar(64) NOT NULL,
    "DisplayName" varchar(200) NOT NULL,
    email varchar(320) NOT NULL,
    "Title" varchar(200) NOT NULL,
    "Department" varchar(200) NOT NULL,
    "ManagerEmployeeId" uuid,
    "Location" varchar(200) NOT NULL,
    "EmploymentType" varchar(32) NOT NULL,
    "Status" varchar(32) NOT NULL,
    "StartDate" date NOT NULL,
    "SeparationDate" date,
    CONSTRAINT ck_employees_employment_type CHECK (
        "EmploymentType" IN ('FullTime','PartTime','Contract','Temporary','Intern')
    ),
    CONSTRAINT ck_employees_status CHECK (
        "Status" IN ('Onboarding','Active','Leave','Suspended','Separated','Archived')
    ),
    CONSTRAINT ck_employees_separation_date CHECK (
        "SeparationDate" IS NULL OR "SeparationDate" >= "StartDate"
    ),
    CONSTRAINT ck_employees_manager_not_self CHECK (
        "ManagerEmployeeId" IS NULL OR "ManagerEmployeeId" <> "Id"
    ),
    CONSTRAINT ux_employees_tenant_identity UNIQUE ("OrganizationId", "Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_employees_organization_number
    ON employees ("OrganizationId", "EmployeeNumber");

CREATE UNIQUE INDEX IF NOT EXISTS ux_employees_organization_email
    ON employees ("OrganizationId", email);

CREATE INDEX IF NOT EXISTS ix_employees_organization_status
    ON employees ("OrganizationId", "Status");

CREATE INDEX IF NOT EXISTS ix_employees_organization_manager
    ON employees ("OrganizationId", "ManagerEmployeeId");

ALTER TABLE employees
    ADD CONSTRAINT fk_employees_manager_same_organization
    FOREIGN KEY ("OrganizationId", "ManagerEmployeeId")
    REFERENCES employees ("OrganizationId", "Id")
    ON DELETE SET NULL;

COMMIT;

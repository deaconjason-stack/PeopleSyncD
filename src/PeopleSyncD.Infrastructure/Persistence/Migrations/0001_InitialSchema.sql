-- PeopleSyncD initial relational schema.
-- Production deployments should execute this through the migration runner.

CREATE SCHEMA IF NOT EXISTS peoplesyncd;

CREATE TABLE IF NOT EXISTS peoplesyncd.organizations (
    id uuid PRIMARY KEY,
    name varchar(200) NOT NULL,
    slug varchar(100) NOT NULL UNIQUE,
    status varchar(32) NOT NULL,
    created_at_utc timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS peoplesyncd.users (
    id uuid PRIMARY KEY,
    email varchar(320) NOT NULL UNIQUE,
    display_name varchar(200) NOT NULL,
    status varchar(32) NOT NULL,
    created_at_utc timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS peoplesyncd.organization_memberships (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES peoplesyncd.users(id),
    organization_id uuid NOT NULL REFERENCES peoplesyncd.organizations(id),
    role varchar(32) NOT NULL,
    created_at_utc timestamptz NOT NULL,
    CONSTRAINT uq_membership_user_organization UNIQUE(user_id, organization_id)
);

CREATE TABLE IF NOT EXISTS peoplesyncd.people (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL REFERENCES peoplesyncd.organizations(id),
    first_name varchar(100) NOT NULL,
    last_name varchar(100) NOT NULL,
    email varchar(320) NOT NULL,
    status varchar(32) NOT NULL,
    created_at_utc timestamptz NOT NULL,
    CONSTRAINT uq_people_organization_email UNIQUE(organization_id, email)
);

CREATE INDEX IF NOT EXISTS ix_people_organization_id ON peoplesyncd.people(organization_id);
CREATE INDEX IF NOT EXISTS ix_memberships_organization_id ON peoplesyncd.organization_memberships(organization_id);

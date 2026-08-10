CREATE TABLE IF NOT EXISTS persons (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    display_name text NOT NULL,
    legal_name_ciphertext bytea,
    preferred_name text,
    row_version bigint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    archived_at timestamptz
);

CREATE TABLE IF NOT EXISTS employment_relationships (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    person_id uuid NOT NULL,
    employment_type text NOT NULL,
    status text NOT NULL,
    start_date date NOT NULL,
    end_date date,
    row_version bigint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS onboarding_instances (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    employment_id uuid NOT NULL,
    template_version_id uuid NOT NULL,
    status text NOT NULL,
    readiness_percent numeric(5,2) NOT NULL DEFAULT 0,
    created_at timestamptz NOT NULL DEFAULT now(),
    completed_at timestamptz
);

ALTER TABLE persons ENABLE ROW LEVEL SECURITY;
ALTER TABLE employment_relationships ENABLE ROW LEVEL SECURITY;
ALTER TABLE onboarding_instances ENABLE ROW LEVEL SECURITY;

CREATE TABLE IF NOT EXISTS worker_assignments (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL,
    person_id uuid NOT NULL,
    position_id uuid,
    department_id uuid,
    supervisor_assignment_id uuid,
    worker_type text NOT NULL,
    employment_status text NOT NULL,
    work_arrangement text,
    start_date date NOT NULL,
    end_date date,
    scheduled_hours_per_week numeric(5,2),
    onboarding_status text NOT NULL DEFAULT 'not_started',
    offboarding_status text NOT NULL DEFAULT 'not_started',
    row_version bigint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    archived_at timestamptz,
    created_by uuid NOT NULL,
    updated_by uuid NOT NULL
);

ALTER TABLE worker_assignments ENABLE ROW LEVEL SECURITY;

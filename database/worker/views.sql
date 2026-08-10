CREATE OR REPLACE VIEW active_worker_directory AS
SELECT
    id,
    organization_id,
    person_id,
    position_id,
    department_id,
    supervisor_assignment_id,
    worker_type,
    employment_status,
    work_arrangement,
    start_date,
    onboarding_status,
    row_version
FROM worker_assignments
WHERE archived_at IS NULL
  AND employment_status IN ('onboarding','active','leave','suspended');

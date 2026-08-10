CREATE OR REPLACE VIEW active_employment_directory AS
SELECT
    id,
    organization_id,
    person_id,
    employment_type,
    status,
    start_date,
    row_version
FROM employment_relationships
WHERE status IN ('onboarding','active','leave','suspended');

CREATE INDEX IF NOT EXISTS idx_persons_org_display_name
    ON persons (organization_id, display_name)
    WHERE archived_at IS NULL;

CREATE INDEX IF NOT EXISTS idx_employment_org_person_status
    ON employment_relationships (organization_id, person_id, status);

CREATE INDEX IF NOT EXISTS idx_onboarding_org_status
    ON onboarding_instances (organization_id, status);

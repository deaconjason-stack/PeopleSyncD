ALTER TABLE employment_relationships
    ADD CONSTRAINT employment_relationships_status_check
    CHECK (status IN ('pending_hire','onboarding','active','leave','suspended','ended','archived'));

ALTER TABLE employment_relationships
    ADD CONSTRAINT employment_relationships_dates_check
    CHECK (end_date IS NULL OR end_date >= start_date);

ALTER TABLE onboarding_instances
    ADD CONSTRAINT onboarding_readiness_check
    CHECK (readiness_percent BETWEEN 0 AND 100);

CREATE POLICY persons_tenant_policy ON persons
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);

CREATE POLICY employment_tenant_policy ON employment_relationships
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);

CREATE POLICY onboarding_tenant_policy ON onboarding_instances
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);

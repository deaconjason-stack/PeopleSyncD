ALTER TABLE worker_assignments
    ADD CONSTRAINT worker_assignments_type_check
    CHECK (worker_type IN ('employee','contractor','volunteer','intern','instructor','advisor'));

ALTER TABLE worker_assignments
    ADD CONSTRAINT worker_assignments_status_check
    CHECK (employment_status IN ('planned','onboarding','active','leave','suspended','ended','archived'));

ALTER TABLE worker_assignments
    ADD CONSTRAINT worker_assignments_dates_check
    CHECK (end_date IS NULL OR end_date >= start_date);

ALTER TABLE worker_assignments
    ADD CONSTRAINT worker_assignments_hours_check
    CHECK (scheduled_hours_per_week IS NULL OR scheduled_hours_per_week BETWEEN 0 AND 168);

CREATE POLICY worker_assignments_tenant_policy ON worker_assignments
    USING (organization_id = current_setting('app.organization_id', true)::uuid)
    WITH CHECK (organization_id = current_setting('app.organization_id', true)::uuid);

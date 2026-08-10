CREATE INDEX IF NOT EXISTS idx_worker_assignments_org_status
    ON worker_assignments (organization_id, employment_status)
    WHERE archived_at IS NULL;

CREATE INDEX IF NOT EXISTS idx_worker_assignments_org_person
    ON worker_assignments (organization_id, person_id);

CREATE INDEX IF NOT EXISTS idx_worker_assignments_supervisor
    ON worker_assignments (organization_id, supervisor_assignment_id)
    WHERE supervisor_assignment_id IS NOT NULL AND archived_at IS NULL;

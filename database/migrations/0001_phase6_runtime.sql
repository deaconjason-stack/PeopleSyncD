BEGIN;

CREATE TABLE IF NOT EXISTS organizations (
  id uuid PRIMARY KEY,
  name text NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS persons (
  id uuid PRIMARY KEY,
  organization_id uuid NOT NULL REFERENCES organizations(id),
  display_name text NOT NULL,
  preferred_name text,
  row_version bigint NOT NULL DEFAULT 1,
  created_at timestamptz NOT NULL DEFAULT now(),
  archived_at timestamptz
);

CREATE TABLE IF NOT EXISTS workers (
  id uuid PRIMARY KEY,
  organization_id uuid NOT NULL REFERENCES organizations(id),
  person_id uuid NOT NULL REFERENCES persons(id),
  worker_type text NOT NULL CHECK (worker_type IN ('employee','contractor','volunteer','intern','instructor','advisor')),
  employment_status text NOT NULL CHECK (employment_status IN ('planned','onboarding','active','leave','suspended','ended','archived')),
  start_date date NOT NULL,
  row_version bigint NOT NULL DEFAULT 1,
  created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS audit_events (
  id uuid PRIMARY KEY,
  organization_id uuid NOT NULL REFERENCES organizations(id),
  actor_id text NOT NULL,
  action text NOT NULL,
  resource_type text NOT NULL,
  resource_id text,
  outcome text NOT NULL CHECK (outcome IN ('success','denied','failure')),
  correlation_id uuid NOT NULL,
  metadata jsonb NOT NULL DEFAULT '{}'::jsonb,
  occurred_at timestamptz NOT NULL DEFAULT now()
);

CREATE OR REPLACE FUNCTION prevent_audit_mutation() RETURNS trigger AS $$
BEGIN
  RAISE EXCEPTION 'audit_events are append-only';
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS audit_events_immutable ON audit_events;
CREATE TRIGGER audit_events_immutable
BEFORE UPDATE OR DELETE ON audit_events
FOR EACH ROW EXECUTE FUNCTION prevent_audit_mutation();

ALTER TABLE persons ENABLE ROW LEVEL SECURITY;
ALTER TABLE workers ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit_events ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS persons_tenant_policy ON persons;
CREATE POLICY persons_tenant_policy ON persons
  USING (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid)
  WITH CHECK (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid);

DROP POLICY IF EXISTS workers_tenant_policy ON workers;
CREATE POLICY workers_tenant_policy ON workers
  USING (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid)
  WITH CHECK (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid);

DROP POLICY IF EXISTS audit_events_tenant_policy ON audit_events;
CREATE POLICY audit_events_tenant_policy ON audit_events
  USING (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid)
  WITH CHECK (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid);

CREATE INDEX IF NOT EXISTS idx_persons_org_display_name ON persons (organization_id, display_name) WHERE archived_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_workers_org_status ON workers (organization_id, employment_status);
CREATE INDEX IF NOT EXISTS idx_audit_org_occurred ON audit_events (organization_id, occurred_at DESC);

INSERT INTO organizations (id, name)
VALUES ('11111111-1111-4111-8111-111111111111', 'MediSyncD Technologies')
ON CONFLICT (id) DO NOTHING;

INSERT INTO persons (id, organization_id, display_name, preferred_name)
VALUES ('22222222-2222-4222-8222-222222222222', '11111111-1111-4111-8111-111111111111', 'Alex Morgan', 'Alex')
ON CONFLICT (id) DO NOTHING;

INSERT INTO workers (id, organization_id, person_id, worker_type, employment_status, start_date)
VALUES ('33333333-3333-4333-8333-333333333333', '11111111-1111-4111-8111-111111111111', '22222222-2222-4222-8222-222222222222', 'employee', 'onboarding', DATE '2026-08-10')
ON CONFLICT (id) DO NOTHING;

COMMIT;

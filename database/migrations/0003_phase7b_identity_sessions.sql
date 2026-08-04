BEGIN;

CREATE TABLE IF NOT EXISTS users (
  id text PRIMARY KEY,
  email text,
  display_name text NOT NULL,
  status text NOT NULL CHECK (status IN ('active','suspended','disabled')),
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email_unique
  ON users (lower(email)) WHERE email IS NOT NULL;

CREATE TABLE IF NOT EXISTS organization_memberships (
  id uuid PRIMARY KEY,
  organization_id uuid NOT NULL REFERENCES organizations(id),
  user_id text NOT NULL REFERENCES users(id),
  role_key text NOT NULL,
  status text NOT NULL CHECK (status IN ('active','suspended','ended')),
  permissions jsonb NOT NULL DEFAULT '[]'::jsonb CHECK (jsonb_typeof(permissions) = 'array'),
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  UNIQUE (organization_id, user_id)
);

CREATE TABLE IF NOT EXISTS identity_sessions (
  id uuid PRIMARY KEY,
  organization_id uuid NOT NULL REFERENCES organizations(id),
  user_id text NOT NULL REFERENCES users(id),
  authentication_methods jsonb NOT NULL DEFAULT '[]'::jsonb CHECK (jsonb_typeof(authentication_methods) = 'array'),
  issued_at timestamptz NOT NULL DEFAULT now(),
  expires_at timestamptz NOT NULL,
  revoked_at timestamptz,
  revoked_by text REFERENCES users(id),
  CHECK (expires_at > issued_at)
);

CREATE TABLE IF NOT EXISTS mfa_methods (
  id uuid PRIMARY KEY,
  organization_id uuid NOT NULL REFERENCES organizations(id),
  user_id text NOT NULL REFERENCES users(id),
  method text NOT NULL CHECK (method IN ('totp','webauthn')),
  label text,
  status text NOT NULL CHECK (status IN ('pending','active','revoked')),
  created_at timestamptz NOT NULL DEFAULT now(),
  verified_at timestamptz,
  revoked_at timestamptz
);

CREATE TABLE IF NOT EXISTS external_identities (
  id uuid PRIMARY KEY,
  user_id text NOT NULL REFERENCES users(id),
  provider_key text NOT NULL,
  issuer text NOT NULL,
  subject text NOT NULL,
  status text NOT NULL CHECK (status IN ('active','disabled')),
  created_at timestamptz NOT NULL DEFAULT now(),
  last_authenticated_at timestamptz,
  UNIQUE (issuer, subject)
);

CREATE TABLE IF NOT EXISTS security_events (
  id uuid PRIMARY KEY,
  organization_id uuid NOT NULL REFERENCES organizations(id),
  user_id text REFERENCES users(id),
  event_type text NOT NULL,
  outcome text NOT NULL CHECK (outcome IN ('success','denied','failure')),
  correlation_id uuid NOT NULL,
  metadata jsonb NOT NULL DEFAULT '{}'::jsonb,
  occurred_at timestamptz NOT NULL DEFAULT now()
);

CREATE OR REPLACE FUNCTION prevent_security_event_mutation() RETURNS trigger AS $$
BEGIN
  RAISE EXCEPTION 'security_events are append-only';
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS security_events_immutable ON security_events;
CREATE TRIGGER security_events_immutable
BEFORE UPDATE OR DELETE ON security_events
FOR EACH ROW EXECUTE FUNCTION prevent_security_event_mutation();

ALTER TABLE organization_memberships ENABLE ROW LEVEL SECURITY;
ALTER TABLE identity_sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE mfa_methods ENABLE ROW LEVEL SECURITY;
ALTER TABLE security_events ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS organization_memberships_tenant_policy ON organization_memberships;
CREATE POLICY organization_memberships_tenant_policy ON organization_memberships
  USING (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid)
  WITH CHECK (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid);

DROP POLICY IF EXISTS identity_sessions_tenant_policy ON identity_sessions;
CREATE POLICY identity_sessions_tenant_policy ON identity_sessions
  USING (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid)
  WITH CHECK (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid);

DROP POLICY IF EXISTS mfa_methods_tenant_policy ON mfa_methods;
CREATE POLICY mfa_methods_tenant_policy ON mfa_methods
  USING (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid)
  WITH CHECK (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid);

DROP POLICY IF EXISTS security_events_tenant_policy ON security_events;
CREATE POLICY security_events_tenant_policy ON security_events
  USING (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid)
  WITH CHECK (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid);

ALTER TABLE organization_memberships FORCE ROW LEVEL SECURITY;
ALTER TABLE identity_sessions FORCE ROW LEVEL SECURITY;
ALTER TABLE mfa_methods FORCE ROW LEVEL SECURITY;
ALTER TABLE security_events FORCE ROW LEVEL SECURITY;

CREATE INDEX IF NOT EXISTS idx_memberships_org_status ON organization_memberships (organization_id, status, user_id);
CREATE INDEX IF NOT EXISTS idx_sessions_org_user_issued ON identity_sessions (organization_id, user_id, issued_at DESC);
CREATE INDEX IF NOT EXISTS idx_sessions_active ON identity_sessions (organization_id, user_id, expires_at) WHERE revoked_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_mfa_org_user_status ON mfa_methods (organization_id, user_id, status);
CREATE INDEX IF NOT EXISTS idx_security_events_org_time ON security_events (organization_id, occurred_at DESC);

INSERT INTO users (id, email, display_name, status)
VALUES ('founder-jason', 'deaconjason@medisyncdtechnologies.com', 'Jason Henderson', 'active')
ON CONFLICT (id) DO UPDATE
SET email = EXCLUDED.email, display_name = EXCLUDED.display_name, status = EXCLUDED.status, updated_at = now();

INSERT INTO organization_memberships
  (id, organization_id, user_id, role_key, status, permissions)
VALUES (
  '44444444-4444-4444-8444-444444444444',
  '11111111-1111-4111-8111-111111111111',
  'founder-jason',
  'founder',
  'active',
  '["founder.dashboard.read","person.read.summary","person.create","worker.read","worker.create","audit.append","audit.read","ai.tool.founder.get_brief","identity.session.read","identity.session.revoke","identity.mfa.read","identity.mfa.enroll","organization.membership.read"]'::jsonb
)
ON CONFLICT (organization_id, user_id) DO UPDATE
SET role_key = EXCLUDED.role_key,
    status = EXCLUDED.status,
    permissions = EXCLUDED.permissions,
    updated_at = now();

GRANT SELECT ON users TO peoplesyncd_runtime;
GRANT SELECT ON organizations TO peoplesyncd_runtime;
GRANT SELECT ON organization_memberships TO peoplesyncd_runtime;
GRANT SELECT, INSERT, UPDATE ON identity_sessions TO peoplesyncd_runtime;
GRANT SELECT, INSERT ON mfa_methods, security_events TO peoplesyncd_runtime;

COMMIT;

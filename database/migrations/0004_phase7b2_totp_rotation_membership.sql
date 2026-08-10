BEGIN;

ALTER TABLE identity_sessions
  ADD COLUMN IF NOT EXISTS session_family_id uuid,
  ADD COLUMN IF NOT EXISTS rotated_from uuid REFERENCES identity_sessions(id),
  ADD COLUMN IF NOT EXISTS replaced_by uuid REFERENCES identity_sessions(id);

UPDATE identity_sessions
SET session_family_id = id
WHERE session_family_id IS NULL;

ALTER TABLE identity_sessions
  ALTER COLUMN session_family_id SET NOT NULL;

ALTER TABLE mfa_methods
  ADD COLUMN IF NOT EXISTS secret_ciphertext text,
  ADD COLUMN IF NOT EXISTS failed_attempts integer NOT NULL DEFAULT 0 CHECK (failed_attempts >= 0),
  ADD COLUMN IF NOT EXISTS last_failed_at timestamptz;

CREATE TABLE IF NOT EXISTS mfa_recovery_codes (
  id uuid PRIMARY KEY,
  organization_id uuid NOT NULL REFERENCES organizations(id),
  user_id text NOT NULL REFERENCES users(id),
  mfa_method_id uuid NOT NULL REFERENCES mfa_methods(id),
  code_hash text NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now(),
  used_at timestamptz,
  UNIQUE (mfa_method_id, code_hash)
);

ALTER TABLE mfa_recovery_codes ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS mfa_recovery_codes_tenant_policy ON mfa_recovery_codes;
CREATE POLICY mfa_recovery_codes_tenant_policy ON mfa_recovery_codes
  USING (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid)
  WITH CHECK (organization_id = NULLIF(current_setting('app.organization_id', true), '')::uuid);
ALTER TABLE mfa_recovery_codes FORCE ROW LEVEL SECURITY;

CREATE INDEX IF NOT EXISTS idx_sessions_family_issued
  ON identity_sessions (organization_id, session_family_id, issued_at DESC);
CREATE INDEX IF NOT EXISTS idx_sessions_rotated_from
  ON identity_sessions (rotated_from) WHERE rotated_from IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_recovery_codes_active
  ON mfa_recovery_codes (organization_id, user_id, mfa_method_id) WHERE used_at IS NULL;

GRANT UPDATE ON organization_memberships TO peoplesyncd_runtime;
GRANT SELECT, INSERT, UPDATE ON mfa_methods TO peoplesyncd_runtime;
GRANT SELECT, INSERT, UPDATE ON mfa_recovery_codes TO peoplesyncd_runtime;

UPDATE organization_memberships
SET permissions = (
      SELECT jsonb_agg(permission ORDER BY permission #>> '{}')
      FROM (
        SELECT DISTINCT permission
        FROM jsonb_array_elements(
          permissions || '["identity.session.rotate","identity.mfa.verify","organization.membership.manage"]'::jsonb
        ) AS permission
      ) AS unique_permissions
    ),
    updated_at = now()
WHERE organization_id = '11111111-1111-4111-8111-111111111111'
  AND user_id = 'founder-jason';

COMMIT;

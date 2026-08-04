BEGIN;

ALTER TABLE mfa_methods
  ADD COLUMN IF NOT EXISTS last_totp_counter bigint;

CREATE INDEX IF NOT EXISTS idx_mfa_last_totp_counter
  ON mfa_methods (organization_id, user_id, last_totp_counter)
  WHERE method = 'totp' AND status = 'active';

CREATE OR REPLACE FUNCTION enforce_last_founder_invariant()
RETURNS trigger
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path = public, pg_temp
AS $$
DECLARE
  alternate_founders integer;
  loses_authority boolean;
BEGIN
  loses_authority :=
    OLD.role_key = 'founder'
    AND OLD.status = 'active'
    AND OLD.permissions @> '["organization.membership.manage"]'::jsonb
    AND (
      NEW.status <> 'active'
      OR NOT (NEW.permissions @> '["organization.membership.manage"]'::jsonb)
    );

  IF loses_authority THEN
    SELECT count(*)
      INTO alternate_founders
      FROM organization_memberships
     WHERE organization_id = OLD.organization_id
       AND user_id <> OLD.user_id
       AND role_key = 'founder'
       AND status = 'active'
       AND permissions @> '["organization.membership.manage"]'::jsonb;

    IF alternate_founders = 0 THEN
      RAISE EXCEPTION 'Last Founder invariant violation'
        USING ERRCODE = 'check_violation';
    END IF;
  END IF;

  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS organization_memberships_last_founder ON organization_memberships;
CREATE TRIGGER organization_memberships_last_founder
BEFORE UPDATE OF status, permissions ON organization_memberships
FOR EACH ROW EXECUTE FUNCTION enforce_last_founder_invariant();

UPDATE organization_memberships
SET permissions = (
      SELECT jsonb_agg(permission ORDER BY permission #>> '{}')
      FROM (
        SELECT DISTINCT permission
        FROM jsonb_array_elements(
          permissions || '["identity.mfa.recovery.consume"]'::jsonb
        ) AS permission
      ) AS unique_permissions
    ),
    updated_at = now()
WHERE organization_id = '11111111-1111-4111-8111-111111111111'
  AND user_id = 'founder-jason';

COMMENT ON ROLE peoplesyncd_runtime IS
  'NOLOGIN capability role. Production application logins must be provisioned separately and granted this role.';

COMMIT;

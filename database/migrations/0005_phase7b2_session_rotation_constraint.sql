BEGIN;

ALTER TABLE identity_sessions
  ALTER CONSTRAINT identity_sessions_replaced_by_fkey
  DEFERRABLE INITIALLY DEFERRED;

COMMIT;

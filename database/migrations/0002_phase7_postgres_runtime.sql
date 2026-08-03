BEGIN;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'peoplesyncd_runtime') THEN
    CREATE ROLE peoplesyncd_runtime NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT;
  END IF;
  EXECUTE format('GRANT peoplesyncd_runtime TO %I', current_user);
END;
$$;

GRANT USAGE ON SCHEMA public TO peoplesyncd_runtime;
GRANT SELECT ON organizations TO peoplesyncd_runtime;
GRANT SELECT, INSERT ON persons, workers, audit_events TO peoplesyncd_runtime;

ALTER TABLE persons FORCE ROW LEVEL SECURITY;
ALTER TABLE workers FORCE ROW LEVEL SECURITY;
ALTER TABLE audit_events FORCE ROW LEVEL SECURITY;

COMMIT;

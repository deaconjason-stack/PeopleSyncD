\set ON_ERROR_STOP on

\if :{?runtime_login}
\else
  \echo 'runtime_login psql variable is required'
  \quit 1
\endif

\if :{?runtime_password}
\else
  \echo 'runtime_password psql variable is required'
  \quit 1
\endif

SELECT format(
  'CREATE ROLE %I LOGIN PASSWORD %L NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION',
  :'runtime_login',
  :'runtime_password'
)
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'runtime_login')
\gexec

SELECT format('ALTER ROLE %I PASSWORD %L', :'runtime_login', :'runtime_password')
\gexec

SELECT format('GRANT peoplesyncd_runtime TO %I', :'runtime_login')
\gexec

SELECT format('ALTER ROLE %I SET statement_timeout = %L', :'runtime_login', '10s')
\gexec

SELECT format('ALTER ROLE %I SET idle_in_transaction_session_timeout = %L', :'runtime_login', '30s')
\gexec

\echo 'Dedicated PeopleSyncD runtime login provisioned. Store the password only in the approved secret manager.'

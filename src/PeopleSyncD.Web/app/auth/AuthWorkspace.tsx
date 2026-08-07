'use client';

import Link from 'next/link';
import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { ApiError, apiRequest } from '../../lib/api';
import type { AccessToken, CurrentSession, OrganizationAccess } from '../../lib/contracts';

const sessionKey = 'peoplesyncd.access-token';

function field(form: FormData, name: string): string {
  return String(form.get(name) ?? '').trim();
}

export default function AuthWorkspace() {
  const [token, setToken] = useState<string>();
  const [session, setSession] = useState<CurrentSession>();
  const [organizations, setOrganizations] = useState<OrganizationAccess[]>([]);
  const [status, setStatus] = useState('Register a tenant or sign in to begin.');
  const [busy, setBusy] = useState(false);

  const applyToken = useCallback(async (value: string) => {
    sessionStorage.setItem(sessionKey, value);
    setToken(value);
    const [current, available] = await Promise.all([
      apiRequest<CurrentSession>('/api/v1/auth/me', {}, value),
      apiRequest<OrganizationAccess[]>('/api/v1/auth/organizations', {}, value),
    ]);
    setSession(current);
    setOrganizations(available);
  }, []);

  useEffect(() => {
    const stored = sessionStorage.getItem(sessionKey);
    if (!stored) return;
    void applyToken(stored).catch(() => {
      sessionStorage.removeItem(sessionKey);
      setToken(undefined);
      setSession(undefined);
      setOrganizations([]);
    });
  }, [applyToken]);

  async function run(action: () => Promise<void>) {
    setBusy(true);
    try {
      await action();
    } catch (error) {
      setStatus(error instanceof ApiError ? error.message : 'The request could not be completed.');
    } finally {
      setBusy(false);
    }
  }

  function register(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    void run(async () => {
      const response = await apiRequest<AccessToken>('/api/v1/auth/register-tenant', {
        method: 'POST',
        body: JSON.stringify({
          organizationName: field(form, 'organizationName'),
          organizationSlug: field(form, 'organizationSlug'),
          displayName: field(form, 'displayName'),
          email: field(form, 'email'),
          password: field(form, 'password'),
        }),
      });
      await applyToken(response.accessToken);
      setStatus('Owner account created. Verify the email address before using tenant permissions.');
    });
  }

  function login(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    void run(async () => {
      const response = await apiRequest<AccessToken>('/api/v1/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email: field(form, 'email'), password: field(form, 'password') }),
      });
      await applyToken(response.accessToken);
      setStatus(response.user.emailConfirmed
        ? 'Authenticated. Select an organization to activate tenant permissions.'
        : 'Authenticated. Verify your email before selecting an organization.');
    });
  }

  function requestVerification() {
    if (!token) return;
    void run(async () => {
      await apiRequest('/api/v1/auth/email-verification/request', { method: 'POST' }, token);
      setStatus('Verification message queued. Local development writes it to the protected .local-email outbox.');
    });
  }

  function selectOrganization(organizationId: string) {
    if (!token) return;
    void run(async () => {
      const response = await apiRequest<AccessToken>('/api/v1/auth/select-organization', {
        method: 'POST',
        body: JSON.stringify({ organizationId }),
      }, token);
      await applyToken(response.accessToken);
      setStatus(`${response.tenant?.organizationName ?? 'Organization'} is now the active tenant.`);
    });
  }

  function signOut() {
    sessionStorage.removeItem(sessionKey);
    setToken(undefined);
    setSession(undefined);
    setOrganizations([]);
    setStatus('Signed out of this browser session.');
  }

  return (
    <section className="workspace" aria-labelledby="workspace-title">
      <div className="workspace-heading">
        <div>
          <p className="eyebrow">Secure workspace</p>
          <h2 id="workspace-title">Platform access</h2>
        </div>
        {token && <button type="button" className="secondary" onClick={signOut}>Sign out</button>}
      </div>

      <p className="status" aria-live="polite">{status}</p>

      {!token && (
        <div className="auth-grid">
          <form className="panel" onSubmit={register}>
            <h3>Create tenant</h3>
            <label>Organization name<input name="organizationName" required maxLength={200} /></label>
            <label>Organization slug<input name="organizationSlug" required maxLength={80} pattern="[a-z0-9]+(?:-[a-z0-9]+)*" /></label>
            <label>Owner name<input name="displayName" required maxLength={200} /></label>
            <label>Email<input name="email" type="email" required maxLength={320} /></label>
            <label>Password<input name="password" type="password" required minLength={12} maxLength={128} /></label>
            <button disabled={busy} type="submit">Create owner account</button>
          </form>

          <form className="panel" onSubmit={login}>
            <h3>Sign in</h3>
            <label>Email<input name="email" type="email" required maxLength={320} /></label>
            <label>Password<input name="password" type="password" required maxLength={128} /></label>
            <button disabled={busy} type="submit">Sign in</button>
          </form>
        </div>
      )}

      {token && session && (
        <div className="auth-grid">
          <article className="panel">
            <h3>Current identity</h3>
            <dl>
              <dt>Name</dt><dd>{session.user.displayName}</dd>
              <dt>Email</dt><dd>{session.user.email}</dd>
              <dt>Email verified</dt><dd>{session.user.emailConfirmed ? 'Yes' : 'No'}</dd>
              <dt>MFA enrolled</dt><dd>{session.user.mfaEnabled ? 'Yes' : 'No'}</dd>
              <dt>Tenant</dt><dd>{session.tenant?.organizationName ?? 'Not selected'}</dd>
              <dt>Role</dt><dd>{session.tenant?.role ?? 'Authenticated user'}</dd>
            </dl>
            {!session.user.emailConfirmed && (
              <button disabled={busy} type="button" onClick={requestVerification}>Send verification</button>
            )}
            {session.tenant && <Link className="primary-link inline-link" href="/members">Manage organization users</Link>}
            <Link className="secondary-link" href="/invite">Accept an invitation</Link>
          </article>
          <article className="panel">
            <h3>Authorized organizations</h3>
            {organizations.length === 0 && <p>No active organization memberships are available.</p>}
            <div className="organization-list">
              {organizations.map((organization) => (
                <button
                  type="button"
                  className="organization"
                  disabled={busy || organization.status !== 'Active' || !session.user.emailConfirmed}
                  key={organization.membershipId}
                  onClick={() => selectOrganization(organization.organizationId)}
                >
                  <strong>{organization.organizationName}</strong>
                  <span>{organization.role} · {organization.status}</span>
                </button>
              ))}
            </div>
          </article>
        </div>
      )}
    </section>
  );
}

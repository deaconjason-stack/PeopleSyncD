'use client';

import Link from 'next/link';
import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { ApiError, apiRequest } from '../../lib/api';
import type {
  AccessToken,
  AccountSecurity,
  MfaChallenge,
  MfaMethod,
  MfaTotpEnrollment,
  PasskeyCeremonyOptions,
  PasskeyCredential,
  RecoveryCodeBatch,
  SecurityEvent,
  SessionSummary,
} from '../../lib/contracts';
import { createPasskeyAssertion, createPasskeyCredential } from '../../lib/webauthn';

const sessionKey = 'peoplesyncd.access-token';

function field(form: FormData, name: string): string {
  return String(form.get(name) ?? '').trim();
}

export default function SecurityWorkspace() {
  const [token, setToken] = useState<string>();
  const [security, setSecurity] = useState<AccountSecurity>();
  const [sessions, setSessions] = useState<SessionSummary[]>([]);
  const [passkeys, setPasskeys] = useState<PasskeyCredential[]>([]);
  const [events, setEvents] = useState<SecurityEvent[]>([]);
  const [enrollment, setEnrollment] = useState<MfaTotpEnrollment>();
  const [recoveryBatch, setRecoveryBatch] = useState<RecoveryCodeBatch>();
  const [challenge, setChallenge] = useState<MfaChallenge>();
  const [method, setMethod] = useState<MfaMethod>('totp');
  const [status, setStatus] = useState('Loading account security…');
  const [busy, setBusy] = useState(false);

  const load = useCallback(async (accessToken: string) => {
    const [securityState, activeSessions, registeredPasskeys, securityEvents] = await Promise.all([
      apiRequest<AccountSecurity>('/api/v1/auth/security', {}, accessToken),
      apiRequest<SessionSummary[]>('/api/v1/auth/sessions', {}, accessToken),
      apiRequest<PasskeyCredential[]>('/api/v1/auth/passkeys', {}, accessToken),
      apiRequest<SecurityEvent[]>('/api/v1/auth/security-events', {}, accessToken),
    ]);
    setSecurity(securityState);
    setSessions(activeSessions);
    setPasskeys(registeredPasskeys);
    setEvents(securityEvents);
    setStatus('Security state loaded from the server.');
  }, []);

  useEffect(() => {
    const stored = sessionStorage.getItem(sessionKey);
    if (!stored) {
      setStatus('Sign in before managing account security.');
      return;
    }
    setToken(stored);
    void load(stored).catch(() => {
      sessionStorage.removeItem(sessionKey);
      setToken(undefined);
      setStatus('This session is no longer valid. Sign in again.');
    });
  }, [load]);

  async function run(action: () => Promise<void>) {
    setBusy(true);
    try {
      await action();
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        setStatus('Recent authentication is required. Re-verify below or sign in again.');
      } else {
        setStatus(error instanceof ApiError ? error.message : error instanceof Error ? error.message : 'The security request could not be completed.');
      }
    } finally {
      setBusy(false);
    }
  }

  function beginEnrollment() {
    if (!token) return;
    void run(async () => {
      const result = await apiRequest<MfaTotpEnrollment>(
        '/api/v1/auth/mfa/totp/enroll',
        { method: 'POST' },
        token,
      );
      setEnrollment(result);
      setStatus('Authenticator secret created. Add it to your authenticator, then verify a current code.');
    });
  }

  function confirmEnrollment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token) return;
    const form = new FormData(event.currentTarget);
    void run(async () => {
      const result = await apiRequest<RecoveryCodeBatch>(
        '/api/v1/auth/mfa/totp/confirm',
        { method: 'POST', body: JSON.stringify({ code: field(form, 'code') }) },
        token,
      );
      setRecoveryBatch(result);
      setEnrollment(undefined);
      setSecurity((current) => current ? {
        ...current,
        mfaEnabled: true,
        passwordOnlyLoginAllowed: false,
        recoveryCodesRemaining: result.recoveryCodes.length,
      } : current);
      sessionStorage.removeItem(sessionKey);
      setToken(undefined);
      setSessions([]);
      setStatus('MFA is enabled and earlier sessions were revoked. Save these recovery codes now, then sign in again.');
    });
  }

  function registerPasskey() {
    if (!token) return;
    void run(async () => {
      const ceremony = await apiRequest<PasskeyCeremonyOptions>(
        '/api/v1/auth/passkeys/registration/options',
        { method: 'POST' },
        token,
      );
      const credentialJson = await createPasskeyCredential(ceremony.publicKeyOptionsJson);
      await apiRequest<PasskeyCredential>('/api/v1/auth/passkeys/registration/complete', {
        method: 'POST',
        body: JSON.stringify({ ceremonyId: ceremony.ceremonyId, credentialJson, displayName: 'Passkey' }),
      }, token);
      await load(token);
      setStatus('Passkey registered. It can now provide phishing-resistant sign-in and step-up authentication.');
    });
  }

  function revokePasskey(credentialId: string) {
    if (!token) return;
    void run(async () => {
      await apiRequest(`/api/v1/auth/passkeys/${credentialId}`, { method: 'DELETE' }, token);
      await load(token);
      setStatus('The passkey was revoked and can no longer authenticate.');
    });
  }

  function passkeyStepUp() {
    if (!token) return;
    void run(async () => {
      const ceremony = await apiRequest<PasskeyCeremonyOptions>(
        '/api/v1/auth/passkeys/step-up/options',
        { method: 'POST' },
        token,
      );
      const credentialJson = await createPasskeyAssertion(ceremony.publicKeyOptionsJson);
      const result = await apiRequest<AccessToken>('/api/v1/auth/passkeys/step-up/complete', {
        method: 'POST',
        body: JSON.stringify({ ceremonyId: ceremony.ceremonyId, credentialJson }),
      }, token);
      sessionStorage.setItem(sessionKey, result.accessToken);
      setToken(result.accessToken);
      await load(result.accessToken);
      setStatus('Passkey step-up succeeded. This session now has fresh phishing-resistant assurance.');
    });
  }

  function regenerateRecoveryCodes() {
    if (!token) return;
    void run(async () => {
      const result = await apiRequest<RecoveryCodeBatch>(
        '/api/v1/auth/mfa/recovery-codes/regenerate',
        { method: 'POST' },
        token,
      );
      setRecoveryBatch(result);
      setSecurity((current) => current ? { ...current, recoveryCodesRemaining: result.recoveryCodes.length } : current);
      setStatus('A new recovery-code batch was generated. Every previous unused recovery code is now revoked.');
    });
  }

  function revokeSession(familyId: string) {
    if (!token) return;
    void run(async () => {
      await apiRequest(`/api/v1/auth/sessions/${familyId}`, { method: 'DELETE' }, token);
      if (sessions.find((session) => session.familyId === familyId)?.isCurrent) {
        sessionStorage.removeItem(sessionKey);
        setToken(undefined);
        setSessions([]);
        setStatus('The current session was revoked. Sign in again to continue.');
        return;
      }
      await load(token);
      setStatus('The selected session was revoked immediately.');
    });
  }

  function revokeOthers() {
    if (!token) return;
    void run(async () => {
      await apiRequest('/api/v1/auth/sessions/revoke-others', { method: 'POST' }, token);
      await load(token);
      setStatus('All other active sessions were revoked.');
    });
  }

  function startStepUp() {
    if (!token) return;
    void run(async () => {
      const result = await apiRequest<MfaChallenge>(
        '/api/v1/auth/mfa/step-up',
        { method: 'POST' },
        token,
      );
      setChallenge(result);
      setMethod(result.methods.includes('totp') ? 'totp' : result.methods[0]);
      setStatus('Complete the fresh MFA challenge to renew session assurance.');
    });
  }

  function completeStepUp(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!challenge) return;
    const form = new FormData(event.currentTarget);
    void run(async () => {
      const result = await apiRequest<AccessToken>('/api/v1/auth/mfa/complete', {
        method: 'POST',
        body: JSON.stringify({
          challengeToken: challenge.challengeToken,
          method,
          code: field(form, 'code'),
        }),
      });
      sessionStorage.setItem(sessionKey, result.accessToken);
      setToken(result.accessToken);
      setChallenge(undefined);
      await load(result.accessToken);
      setStatus('Step-up verification succeeded. The new session is MFA-assured.');
    });
  }

  return (
    <section className="workspace" aria-labelledby="security-title">
      <div className="workspace-heading">
        <div><p className="eyebrow">Account controls</p><h2 id="security-title">Security & sessions</h2></div>
        {!token && <Link className="primary-link" href="/auth">Sign in</Link>}
      </div>
      <p className="status" aria-live="polite">{status}</p>

      {recoveryBatch && (
        <article className="panel">
          <h3>Save these recovery codes now</h3>
          <p>Each code works once. PeopleSyncD stores only hashes and cannot show this batch again.</p>
          <ol className="recovery-codes">{recoveryBatch.recoveryCodes.map((code) => <li key={code}><code>{code}</code></li>)}</ol>
          <p>Generated {new Date(recoveryBatch.generatedAt).toLocaleString()}.</p>
        </article>
      )}

      {security && (
        <div className="auth-grid">
          <article className="panel">
            <h3>Multi-factor authentication</h3>
            <dl>
              <dt>Email verified</dt><dd>{security.emailConfirmed ? 'Yes' : 'No'}</dd>
              <dt>MFA enabled</dt><dd>{security.mfaEnabled ? 'Yes' : 'No'}</dd>
              <dt>Recovery codes remaining</dt><dd>{security.recoveryCodesRemaining}</dd>
            </dl>
            {!security.mfaEnabled && !enrollment && token && <button disabled={busy} type="button" onClick={beginEnrollment}>Set up authenticator</button>}
            {security.mfaEnabled && token && (
              <div className="actions">
                <button disabled={busy} type="button" onClick={regenerateRecoveryCodes}>Regenerate recovery codes</button>
                <button disabled={busy} className="secondary" type="button" onClick={startStepUp}>Re-verify MFA</button>
              </div>
            )}
          </article>
          <article className="panel">
            <h3>Passkeys</h3>
            <p>Passkeys use WebAuthn with required user verification and provide phishing-resistant assurance.</p>
            <div className="actions">
              <button disabled={busy || !token} type="button" onClick={registerPasskey}>Register passkey</button>
              {passkeys.length > 0 && <button disabled={busy} className="secondary" type="button" onClick={passkeyStepUp}>Verify with passkey</button>}
            </div>
            {passkeys.length === 0 ? <p>No active passkeys are registered.</p> : (
              <div className="session-list">
                {passkeys.map((passkey) => (
                  <div className="session-card" key={passkey.id}>
                    <div>
                      <strong>{passkey.displayName}</strong>
                      <p>Registered {new Date(passkey.createdAt).toLocaleString()}</p>
                      <p>{passkey.lastUsedAt ? `Last used ${new Date(passkey.lastUsedAt).toLocaleString()}` : 'Not used yet'} · {passkey.backedUp ? 'Synced/backed up' : 'Not reported as backed up'}</p>
                    </div>
                    <button disabled={busy} className="danger" type="button" onClick={() => revokePasskey(passkey.id)}>Revoke</button>
                  </div>
                ))}
              </div>
            )}
          </article>
        </div>
      )}

      {enrollment && token && (
        <form className="panel" onSubmit={confirmEnrollment}>
          <h3>Connect your authenticator</h3>
          <p>Manual entry key:</p><p className="secret-value"><code>{enrollment.manualEntryKey}</code></p>
          <details><summary>Show OTPAuth URI</summary><p className="secret-value"><code>{enrollment.otpauthUri}</code></p></details>
          <label>Current authenticator code<input name="code" required autoComplete="one-time-code" inputMode="numeric" maxLength={12} /></label>
          <button disabled={busy} type="submit">Verify and enable MFA</button>
        </form>
      )}

      {challenge && token && (
        <form className="panel" onSubmit={completeStepUp}>
          <h3>Fresh MFA verification</h3>
          <label>Method<select value={method} onChange={(event) => setMethod(event.target.value as MfaMethod)}>{challenge.methods.map((available) => <option key={available} value={available}>{available === 'totp' ? 'Authenticator code' : 'Recovery code'}</option>)}</select></label>
          <label>Verification code<input name="code" required autoComplete="one-time-code" maxLength={32} /></label>
          <div className="actions"><button disabled={busy} type="submit">Complete verification</button><button type="button" className="secondary" onClick={() => setChallenge(undefined)}>Cancel</button></div>
        </form>
      )}

      {token && (
        <article className="panel">
          <div className="workspace-heading"><div><h3>Active sessions</h3><p>Privileged operations require authentication within five minutes; refresh rotation does not reset that clock.</p></div><button disabled={busy} className="secondary" type="button" onClick={revokeOthers}>Revoke all others</button></div>
          {sessions.length === 0 && <p>No active sessions were returned.</p>}
          <div className="session-list">
            {sessions.map((session) => (
              <div className="session-card" key={session.familyId}>
                <div>
                  <strong>{session.isCurrent ? 'Current session' : 'Active session'}</strong>
                  <p>{session.deviceLabel ?? 'Unlabeled client'}</p>
                  <p>{session.assuranceLevel.replace('_', ' ').toUpperCase()} · {session.authenticationMethod} · authenticated {session.authenticatedAt ? new Date(session.authenticatedAt).toLocaleString() : 'unknown'} · last seen {new Date(session.lastSeenAt).toLocaleString()}</p>
                </div>
                <button disabled={busy} className={session.isCurrent ? 'danger' : 'secondary'} type="button" onClick={() => revokeSession(session.familyId)}>{session.isCurrent ? 'Revoke current' : 'Revoke'}</button>
              </div>
            ))}
          </div>
        </article>
      )}

      {events.length > 0 && (
        <article className="panel">
          <h3>Recent security events</h3>
          <div className="table-wrap"><table><thead><tr><th>When</th><th>Event</th><th>Target</th></tr></thead><tbody>{events.map((event, index) => <tr key={`${event.occurredAt}-${event.eventType}-${index}`}><td>{new Date(event.occurredAt).toLocaleString()}</td><td>{event.eventType}</td><td>{event.targetType} · {event.targetId}</td></tr>)}</tbody></table></div>
        </article>
      )}
    </section>
  );
}

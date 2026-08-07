'use client';

import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { ApiError, apiRequest } from '../../lib/api';
import type { CurrentSession, Invitation, MembershipAdmin, MembershipStatus, TenantRole } from '../../lib/contracts';

const sessionKey = 'peoplesyncd.access-token';
const roles: TenantRole[] = ['Administrator', 'Manager', 'Member', 'Auditor'];

export default function MembersWorkspace() {
  const [token, setToken] = useState<string>();
  const [session, setSession] = useState<CurrentSession>();
  const [members, setMembers] = useState<MembershipAdmin[]>([]);
  const [invitations, setInvitations] = useState<Invitation[]>([]);
  const [status, setStatus] = useState('Select an organization before opening this workspace.');
  const [busy, setBusy] = useState(false);

  const load = useCallback(async (value: string) => {
    const current = await apiRequest<CurrentSession>('/api/v1/auth/me', {}, value);
    setSession(current);
    if (!current.tenant) {
      setStatus('No tenant is selected. Return to platform access and select an organization.');
      return;
    }

    const base = `/api/v1/organizations/${current.tenant.organizationId}`;
    const [memberList, invitationList] = await Promise.all([
      apiRequest<MembershipAdmin[]>(`${base}/members`, {}, value),
      apiRequest<Invitation[]>(`${base}/invitations`, {}, value),
    ]);
    setMembers(memberList);
    setInvitations(invitationList);
    setStatus(`${memberList.length} membership records loaded.`);
  }, []);

  useEffect(() => {
    const stored = sessionStorage.getItem(sessionKey);
    if (!stored) return;
    setToken(stored);
    void load(stored).catch((error) => setStatus(error instanceof ApiError ? error.message : 'Unable to load members.'));
  }, [load]);

  async function run(action: () => Promise<void>) {
    setBusy(true);
    try {
      await action();
      if (token) await load(token);
    } catch (error) {
      setStatus(error instanceof ApiError ? error.message : 'The administration request failed.');
    } finally {
      setBusy(false);
    }
  }

  function invite(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !session?.tenant) return;
    const form = new FormData(event.currentTarget);
    void run(async () => {
      await apiRequest(`${`/api/v1/organizations/${session.tenant!.organizationId}`}/invitations`, {
        method: 'POST',
        body: JSON.stringify({
          email: String(form.get('email') ?? '').trim(),
          displayName: String(form.get('displayName') ?? '').trim(),
          role: String(form.get('role') ?? 'Member'),
        }),
      }, token);
      event.currentTarget.reset();
      setStatus('Invitation created and queued for delivery.');
    });
  }

  function updateMembership(membershipId: string, role: TenantRole | null, membershipStatus: MembershipStatus | null) {
    if (!token || !session?.tenant) return;
    void run(async () => {
      await apiRequest(`/api/v1/organizations/${session.tenant!.organizationId}/members/${membershipId}`, {
        method: 'PATCH',
        body: JSON.stringify({ role, status: membershipStatus }),
      }, token);
      setStatus('Membership updated. Existing tenant refresh sessions were revoked.');
    });
  }

  const canWrite = session?.tenant?.permissions.includes('memberships.write') ?? false;

  return (
    <section className="workspace" aria-labelledby="members-title">
      <div className="workspace-heading"><h2 id="members-title">Organization users</h2></div>
      <p className="status" aria-live="polite">{status}</p>

      {canWrite && (
        <form className="panel inline-form" onSubmit={invite}>
          <h3>Invite a user</h3>
          <label>Name<input name="displayName" required maxLength={200} /></label>
          <label>Email<input name="email" type="email" required maxLength={320} /></label>
          <label>Role<select name="role" defaultValue="Member">{roles.map((role) => <option key={role}>{role}</option>)}</select></label>
          <button disabled={busy} type="submit">Send invitation</button>
        </form>
      )}

      <div className="table-wrap">
        <table>
          <thead><tr><th>User</th><th>Role</th><th>Status</th><th>Security</th><th>Actions</th></tr></thead>
          <tbody>
            {members.map((member) => (
              <tr key={member.membershipId}>
                <td><strong>{member.displayName}</strong><br /><span>{member.email}</span></td>
                <td>{member.role}</td>
                <td>{member.status}</td>
                <td>{member.emailConfirmed ? 'Verified' : 'Unverified'} · {member.mfaEnabled ? 'MFA' : 'Password only'}</td>
                <td className="actions">
                  {canWrite && member.role !== 'Owner' && member.status !== 'Revoked' && (
                    <>
                      <select
                        aria-label={`Role for ${member.displayName}`}
                        value={member.role}
                        disabled={busy}
                        onChange={(event) => updateMembership(member.membershipId, event.target.value as TenantRole, null)}
                      >
                        {roles.map((role) => <option key={role}>{role}</option>)}
                      </select>
                      {member.status === 'Active'
                        ? <button className="secondary" type="button" disabled={busy} onClick={() => updateMembership(member.membershipId, null, 'Suspended')}>Suspend</button>
                        : <button className="secondary" type="button" disabled={busy} onClick={() => updateMembership(member.membershipId, null, 'Active')}>Reactivate</button>}
                      <button className="danger" type="button" disabled={busy} onClick={() => updateMembership(member.membershipId, null, 'Revoked')}>Revoke</button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <article className="panel">
        <h3>Invitation history</h3>
        <ul className="compact-list">
          {invitations.map((invitation) => (
            <li key={invitation.id}>{invitation.email} · {invitation.role} · {invitation.status}</li>
          ))}
        </ul>
      </article>
    </section>
  );
}

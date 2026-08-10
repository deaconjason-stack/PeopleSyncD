'use client';

import { type FormEvent, useState } from 'react';
import { ApiError, apiRequest } from '../../lib/api';
import type { OrganizationAccess } from '../../lib/contracts';

export default function InviteAcceptance() {
  const [status, setStatus] = useState('Enter the secure invitation token delivered to your email address.');
  const [busy, setBusy] = useState(false);

  function accept(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    setBusy(true);
    void apiRequest<OrganizationAccess>('/api/v1/invitations/accept', {
      method: 'POST',
      body: JSON.stringify({
        token: String(form.get('token') ?? '').trim(),
        displayName: String(form.get('displayName') ?? '').trim(),
        password: String(form.get('password') ?? ''),
      }),
    }).then((access) => {
      setStatus(`Invitation accepted for ${access.organizationName}. Sign in to continue.`);
      event.currentTarget.reset();
    }).catch((error) => {
      setStatus(error instanceof ApiError ? error.message : 'The invitation could not be accepted.');
    }).finally(() => setBusy(false));
  }

  return (
    <section className="workspace" aria-labelledby="accept-title">
      <h2 id="accept-title">Accept invitation</h2>
      <p className="status" aria-live="polite">{status}</p>
      <form className="panel" onSubmit={accept}>
        <label>Invitation token<input name="token" required autoComplete="off" /></label>
        <label>Your name<input name="displayName" required maxLength={200} /></label>
        <label>Create password<input name="password" type="password" required minLength={12} maxLength={128} /></label>
        <button disabled={busy} type="submit">Accept invitation</button>
      </form>
    </section>
  );
}

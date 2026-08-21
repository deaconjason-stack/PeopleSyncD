'use client';

import Link from 'next/link';
import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { ApiError } from '../../../lib/api';
import {
  changeEmploymentStatus,
  getEmployee,
  readAccessToken,
  updateEmployee,
  type Employee,
  type EmploymentStatus,
  type EmploymentType,
} from '../../../lib/hr-api';

const employmentTypes: EmploymentType[] = ['FullTime', 'PartTime', 'Contract', 'Temporary', 'Intern'];

function field(form: FormData, name: string): string {
  return String(form.get(name) ?? '').trim();
}

export default function EmployeeWorkspace({ employeeId }: { employeeId: string }) {
  const [token, setToken] = useState<string>();
  const [employee, setEmployee] = useState<Employee>();
  const [message, setMessage] = useState('Loading employee…');
  const [busy, setBusy] = useState(false);
  const [separationDate, setSeparationDate] = useState('');

  const load = useCallback(async (accessToken: string) => {
    setBusy(true);
    try {
      const data = await getEmployee(accessToken, employeeId);
      setEmployee(data);
      setSeparationDate(data.separationDate ?? '');
      setMessage(`${data.displayName} loaded.`);
    } catch (error) {
      setMessage(error instanceof ApiError ? error.message : 'Unable to load this employee.');
    } finally {
      setBusy(false);
    }
  }, [employeeId]);

  useEffect(() => {
    const accessToken = readAccessToken();
    setToken(accessToken);
    if (!accessToken) {
      setMessage('Sign in and select an organization to view this employee.');
      return;
    }
    void load(accessToken);
  }, [load]);

  function saveProfile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || !employee) return;
    const form = new FormData(event.currentTarget);
    setBusy(true);
    void updateEmployee(token, employee.id, {
      displayName: field(form, 'displayName'),
      email: field(form, 'email'),
      title: field(form, 'title'),
      department: field(form, 'department'),
      managerEmployeeId: null,
      location: field(form, 'location'),
      employmentType: field(form, 'employmentType') as EmploymentType,
    }).then((updated) => {
      setEmployee(updated);
      setMessage('Employee profile saved.');
    }).catch((error: unknown) => {
      setMessage(error instanceof ApiError ? error.message : 'Unable to save this employee.');
    }).finally(() => setBusy(false));
  }

  function transition(status: EmploymentStatus) {
    if (!token || !employee) return;
    if (status === 'Separated' && !separationDate) {
      setMessage('Choose a separation date before separating this employee.');
      return;
    }
    setBusy(true);
    void changeEmploymentStatus(token, employee.id, {
      status,
      separationDate: status === 'Separated' ? separationDate : null,
    }).then((updated) => {
      setEmployee(updated);
      setMessage(`Employment status changed to ${updated.status}.`);
    }).catch((error: unknown) => {
      setMessage(error instanceof ApiError ? error.message : 'Unable to change employment status.');
    }).finally(() => setBusy(false));
  }

  return (
    <section className="workspace" aria-labelledby="employee-title">
      <div className="workspace-heading">
        <div>
          <p className="eyebrow">Employee profile</p>
          <h2 id="employee-title">{employee?.displayName ?? 'Employee'}</h2>
        </div>
        <div className="actions">
          <Link className="secondary-link" href="/people">Back to People</Link>
          <Link className="secondary-link" href="/auth">Account</Link>
        </div>
      </div>

      <p className="status" aria-live="polite">{message}</p>

      {!token && <p><Link className="primary-link" href="/auth">Sign in to continue</Link></p>}

      {employee && (
        <>
          <article className="panel">
            <h3>Employment overview</h3>
            <dl>
              <dt>Employee #</dt><dd>{employee.employeeNumber}</dd>
              <dt>Status</dt><dd>{employee.status}</dd>
              <dt>Start date</dt><dd>{employee.startDate}</dd>
              <dt>Separation date</dt><dd>{employee.separationDate ?? '—'}</dd>
              <dt>Organization</dt><dd>{employee.organizationId}</dd>
            </dl>
          </article>

          <form className="panel" onSubmit={saveProfile}>
            <h3>Profile</h3>
            <div className="auth-grid">
              <label>Display name<input name="displayName" required maxLength={200} defaultValue={employee.displayName} /></label>
              <label>Work email<input name="email" type="email" required maxLength={320} defaultValue={employee.email} /></label>
              <label>Title<input name="title" required maxLength={200} defaultValue={employee.title} /></label>
              <label>Department<input name="department" required maxLength={200} defaultValue={employee.department} /></label>
              <label>Location<input name="location" required maxLength={200} defaultValue={employee.location} /></label>
              <label>
                Employment type
                <select name="employmentType" defaultValue={employee.employmentType}>
                  {employmentTypes.map((type) => <option key={type} value={type}>{type}</option>)}
                </select>
              </label>
            </div>
            <button disabled={busy} type="submit">Save profile</button>
          </form>

          <article className="panel">
            <h3>Employment lifecycle</h3>
            <p>Lifecycle changes are explicit, validated by the domain model, and recorded in the audit stream.</p>
            <label>
              Separation date
              <input
                type="date"
                value={separationDate}
                min={employee.startDate}
                onChange={(event) => setSeparationDate(event.target.value)}
              />
            </label>
            <div className="actions">
              {employee.status === 'Onboarding' && <button disabled={busy} onClick={() => transition('Active')}>Activate</button>}
              {employee.status === 'Active' && <button disabled={busy} onClick={() => transition('Leave')}>Place on leave</button>}
              {employee.status === 'Leave' && <button disabled={busy} onClick={() => transition('Active')}>Return from leave</button>}
              {employee.status === 'Active' && <button disabled={busy} className="secondary" onClick={() => transition('Suspended')}>Suspend</button>}
              {!['Separated', 'Archived'].includes(employee.status) && (
                <button disabled={busy} className="danger" onClick={() => transition('Separated')}>Separate</button>
              )}
              {employee.status === 'Separated' && <button disabled={busy} className="secondary" onClick={() => transition('Archived')}>Archive</button>}
            </div>
          </article>

          <article className="panel">
            <h3>Demo-ready HR modules</h3>
            <p>Onboarding, credentials, documents, cases, and employee activity will attach to this profile as the next vertical slices are completed.</p>
          </article>
        </>
      )}
    </section>
  );
}

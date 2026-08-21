'use client';

import Link from 'next/link';
import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { ApiError } from '../../lib/api';
import {
  createEmployee,
  listEmployees,
  readAccessToken,
  type Employee,
  type EmploymentStatus,
  type EmploymentType,
} from '../../lib/hr-api';

const statuses: Array<EmploymentStatus | ''> = ['', 'Onboarding', 'Active', 'Leave', 'Suspended', 'Separated', 'Archived'];
const employmentTypes: EmploymentType[] = ['FullTime', 'PartTime', 'Contract', 'Temporary', 'Intern'];

function field(form: FormData, name: string): string {
  return String(form.get(name) ?? '').trim();
}

export default function PeopleWorkspace() {
  const [token, setToken] = useState<string>();
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<EmploymentStatus | ''>('');
  const [message, setMessage] = useState('Loading PeopleSyncD workforce…');
  const [busy, setBusy] = useState(false);

  const load = useCallback(async (
    accessToken: string,
    nextSearch = '',
    nextStatus: EmploymentStatus | '' = '',
  ) => {
    setBusy(true);
    try {
      const data = await listEmployees(accessToken, {
        search: nextSearch || undefined,
        status: nextStatus || undefined,
      });
      setEmployees(data);
      setMessage(data.length === 1 ? '1 employee found.' : `${data.length} employees found.`);
    } catch (error) {
      setMessage(error instanceof ApiError ? error.message : 'Unable to load employees.');
    } finally {
      setBusy(false);
    }
  }, []);

  useEffect(() => {
    const accessToken = readAccessToken();
    setToken(accessToken);
    if (!accessToken) {
      setMessage('Sign in and select an organization to open the People workspace.');
      return;
    }
    void load(accessToken);
  }, [load]);

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (token) void load(token, search, statusFilter);
  }

  function create(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token) return;
    const formElement = event.currentTarget;
    const form = new FormData(formElement);
    setBusy(true);
    void createEmployee(token, {
      employeeNumber: field(form, 'employeeNumber'),
      displayName: field(form, 'displayName'),
      email: field(form, 'email'),
      title: field(form, 'title'),
      department: field(form, 'department'),
      managerEmployeeId: field(form, 'managerEmployeeId') || null,
      location: field(form, 'location'),
      employmentType: field(form, 'employmentType') as EmploymentType,
      startDate: field(form, 'startDate'),
    }).then(async (created) => {
      formElement.reset();
      setSearch('');
      setStatusFilter('');
      setMessage(`${created.displayName} was added to PeopleSyncD.`);
      await load(token);
    }).catch((error: unknown) => {
      setMessage(error instanceof ApiError ? error.message : 'Unable to create the employee.');
    }).finally(() => setBusy(false));
  }

  return (
    <section className="workspace" aria-labelledby="people-title">
      <div className="workspace-heading">
        <div>
          <p className="eyebrow">HR workspace</p>
          <h2 id="people-title">People</h2>
        </div>
        <div className="actions">
          <Link className="secondary-link" href="/">Home</Link>
          <Link className="secondary-link" href="/auth">Account</Link>
        </div>
      </div>

      <p className="status" aria-live="polite">{message}</p>

      {!token ? (
        <p><Link className="primary-link" href="/auth">Sign in to continue</Link></p>
      ) : (
        <>
          <form className="inline-form panel" onSubmit={applyFilters}>
            <h3>Find employees</h3>
            <label>
              Search
              <input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Name, email, title, department"
                maxLength={200}
              />
            </label>
            <label>
              Status
              <select
                value={statusFilter}
                onChange={(event) => setStatusFilter(event.target.value as EmploymentStatus | '')}
              >
                {statuses.map((status) => <option key={status || 'all'} value={status}>{status || 'All statuses'}</option>)}
              </select>
            </label>
            <button type="submit" disabled={busy}>Apply filters</button>
          </form>

          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Role</th>
                  <th>Department</th>
                  <th>Location</th>
                  <th>Status</th>
                  <th>Start</th>
                </tr>
              </thead>
              <tbody>
                {employees.map((employee) => (
                  <tr key={employee.id}>
                    <td>
                      <Link href={`/people/${employee.id}`}>{employee.displayName}</Link><br />
                      <span>{employee.employeeNumber} · {employee.email}</span>
                    </td>
                    <td>{employee.title}</td>
                    <td>{employee.department}</td>
                    <td>{employee.location}</td>
                    <td>{employee.status}</td>
                    <td>{employee.startDate}</td>
                  </tr>
                ))}
                {!employees.length && (
                  <tr><td colSpan={6}>No employees match the current filters.</td></tr>
                )}
              </tbody>
            </table>
          </div>

          <form className="panel" onSubmit={create}>
            <h3>Add employee</h3>
            <div className="auth-grid">
              <label>Employee number<input name="employeeNumber" required maxLength={64} /></label>
              <label>Display name<input name="displayName" required maxLength={200} /></label>
              <label>Work email<input name="email" type="email" required maxLength={320} /></label>
              <label>Title<input name="title" required maxLength={200} /></label>
              <label>Department<input name="department" required maxLength={200} /></label>
              <label>Location<input name="location" required maxLength={200} /></label>
              <label>
                Employment type
                <select name="employmentType" defaultValue="FullTime">
                  {employmentTypes.map((type) => <option key={type} value={type}>{type}</option>)}
                </select>
              </label>
              <label>Start date<input name="startDate" type="date" required /></label>
              <label>
                Manager
                <select name="managerEmployeeId" defaultValue="">
                  <option value="">No manager assigned</option>
                  {employees.filter((employee) => employee.status !== 'Archived').map((employee) => (
                    <option key={employee.id} value={employee.id}>{employee.displayName}</option>
                  ))}
                </select>
              </label>
            </div>
            <button disabled={busy} type="submit">Add employee</button>
          </form>
        </>
      )}
    </section>
  );
}

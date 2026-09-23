import { apiRequest } from './api';

export type EmploymentStatus = 'Onboarding' | 'Active' | 'Leave' | 'Suspended' | 'Separated' | 'Archived';
export type EmploymentType = 'FullTime' | 'PartTime' | 'Contract' | 'Temporary' | 'Intern';

export interface Employee {
  id: string;
  organizationId: string;
  employeeNumber: string;
  displayName: string;
  email: string;
  title: string;
  department: string;
  managerEmployeeId?: string | null;
  location: string;
  employmentType: EmploymentType;
  status: EmploymentStatus;
  startDate: string;
  separationDate?: string | null;
}

export interface CreateEmployeeInput {
  employeeNumber: string;
  displayName: string;
  email: string;
  title: string;
  department: string;
  managerEmployeeId?: string | null;
  location: string;
  employmentType: EmploymentType;
  startDate: string;
}

export type UpdateEmployeeInput = Omit<CreateEmployeeInput, 'employeeNumber' | 'startDate'>;

export interface ChangeEmploymentStatusInput {
  status: EmploymentStatus;
  separationDate?: string | null;
}

export const peopleSyncDSessionKey = 'peoplesyncd.access-token';

export function readAccessToken(): string | undefined {
  if (typeof window === 'undefined') return undefined;
  return window.sessionStorage.getItem(peopleSyncDSessionKey) ?? undefined;
}

export function listEmployees(
  accessToken: string,
  filters: { search?: string; status?: EmploymentStatus } = {},
): Promise<Employee[]> {
  const query = new URLSearchParams();
  if (filters.search?.trim()) query.set('search', filters.search.trim());
  if (filters.status) query.set('status', filters.status);
  const suffix = query.size ? `?${query.toString()}` : '';
  return apiRequest<Employee[]>(`/api/v1/employees${suffix}`, {}, accessToken);
}

export function getEmployee(accessToken: string, employeeId: string): Promise<Employee> {
  return apiRequest<Employee>(`/api/v1/employees/${employeeId}`, {}, accessToken);
}

export function createEmployee(accessToken: string, input: CreateEmployeeInput): Promise<Employee> {
  return apiRequest<Employee>('/api/v1/employees', {
    method: 'POST',
    body: JSON.stringify(input),
  }, accessToken);
}

export function updateEmployee(
  accessToken: string,
  employeeId: string,
  input: UpdateEmployeeInput,
): Promise<Employee> {
  return apiRequest<Employee>(`/api/v1/employees/${employeeId}`, {
    method: 'PUT',
    body: JSON.stringify(input),
  }, accessToken);
}

export function changeEmploymentStatus(
  accessToken: string,
  employeeId: string,
  input: ChangeEmploymentStatusInput,
): Promise<Employee> {
  return apiRequest<Employee>(`/api/v1/employees/${employeeId}/status`, {
    method: 'POST',
    body: JSON.stringify(input),
  }, accessToken);
}

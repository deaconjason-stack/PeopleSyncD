export type Person = {
  id: string;
  organizationId: string;
  firstName: string;
  lastName: string;
  email: string;
  status: string;
};

export type CurrentUser = {
  id: string;
  email: string;
  displayName: string;
};

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...(init?.headers ?? {}) },
    credentials: "include",
    cache: "no-store",
  });
  if (!response.ok) throw new Error(`PeopleSyncD API request failed: ${response.status}`);
  return response.json() as Promise<T>;
}

export function getCurrentUser() {
  return request<CurrentUser>("/api/v1/me");
}

export function getPeople(organizationId: string) {
  return request<Person[]>(`/api/v1/people?organizationId=${encodeURIComponent(organizationId)}`);
}

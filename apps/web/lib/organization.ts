export type Organization = {
  id: string;
  name: string;
  slug: string;
  status: string;
  createdAtUtc: string;
};

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

export async function getOrganization(organizationId: string): Promise<Organization> {
  const response = await fetch(`${API_BASE_URL}/api/v1/organizations/${encodeURIComponent(organizationId)}`, {
    credentials: "include",
    cache: "no-store",
  });
  if (!response.ok) throw new Error(`Unable to load organization: ${response.status}`);
  return response.json() as Promise<Organization>;
}

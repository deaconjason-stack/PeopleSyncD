import type { FounderDashboard, PersonSummary, WorkerSummary } from "@peoplesyncd/shared";

export interface PeopleSyncDClientOptions {
  baseUrl: string;
  token: string;
  organizationId: string;
}

export class PeopleSyncDClient {
  constructor(private readonly options: PeopleSyncDClientOptions) {}

  private async request<T>(path: string, init?: RequestInit): Promise<T> {
    const response = await fetch(`${this.options.baseUrl}${path}`, {
      ...init,
      headers: {
        "content-type": "application/json",
        authorization: `Bearer ${this.options.token}`,
        "x-organization-id": this.options.organizationId,
        ...(init?.headers ?? {})
      }
    });
    if (!response.ok) {
      const body = await response.text();
      throw new Error(`PeopleSyncD API ${response.status}: ${body}`);
    }
    if (response.status === 204) return undefined as T;
    return response.json() as Promise<T>;
  }

  founderDashboard(): Promise<FounderDashboard> {
    return this.request("/v1/founder/dashboard");
  }

  persons(): Promise<PersonSummary[]> {
    return this.request("/v1/persons");
  }

  workers(): Promise<WorkerSummary[]> {
    return this.request("/v1/workers");
  }
}

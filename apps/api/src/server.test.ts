import { describe, expect, it } from "vitest";
import { buildServer } from "./server";
import type { ApiConfig } from "./config";

const config: ApiConfig = {
  host: "127.0.0.1",
  port: 0,
  nodeEnv: "test",
  sessionSecret: "test-secret-at-least-sixteen-characters",
  devAuthEnabled: true,
  corsOrigin: "http://localhost:5173",
  storageMode: "memory"
};

async function session(app: ReturnType<typeof buildServer>) {
  const response = await app.inject({ method: "POST", url: "/v1/auth/dev-session" });
  expect(response.statusCode).toBe(200);
  return response.json() as { token: string; organizationId: string };
}

describe("PeopleSyncD API", () => {
  it("serves health checks", async () => {
    const app = buildServer(config);
    const response = await app.inject({ method: "GET", url: "/health/ready" });
    expect(response.statusCode).toBe(200);
    expect(response.json().certified).toBe(false);
    expect(response.json().storage).toBe("in-memory");
    await app.close();
  });

  it("returns the authoritative board on Founder Dashboard", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const response = await app.inject({
      method: "GET",
      url: "/v1/founder/dashboard",
      headers: {
        authorization: `Bearer ${auth.token}`,
        "x-organization-id": auth.organizationId
      }
    });
    expect(response.statusCode).toBe(200);
    expect(response.json().board.map((member: { displayName: string }) => member.displayName)).toEqual([
      "Jason Henderson",
      "Domonique Danielle Henderson",
      "Marietta Jessup"
    ]);
    await app.close();
  });

  it("rejects cross-tenant requests", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const response = await app.inject({
      method: "GET",
      url: "/v1/persons",
      headers: {
        authorization: `Bearer ${auth.token}`,
        "x-organization-id": "99999999-9999-4999-8999-999999999999"
      }
    });
    expect(response.statusCode).toBe(403);
    await app.close();
  });

  it("creates a person and emits audit evidence", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const headers = {
      authorization: `Bearer ${auth.token}`,
      "x-organization-id": auth.organizationId
    };
    const created = await app.inject({
      method: "POST",
      url: "/v1/persons",
      headers,
      payload: { displayName: "Taylor Reed", preferredName: "Taylor" }
    });
    expect(created.statusCode).toBe(201);
    const audit = await app.inject({ method: "GET", url: "/v1/audit/events", headers });
    expect(audit.json().some((event: { action: string }) => event.action === "person.create")).toBe(true);
    await app.close();
  });

  it("invokes only the registered read-only Founder Brief tool", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const headers = {
      authorization: `Bearer ${auth.token}`,
      "x-organization-id": auth.organizationId
    };
    const response = await app.inject({ method: "POST", url: "/v1/ai/tools/founder.get_brief/invoke", headers });
    expect(response.statusCode).toBe(200);
    expect(response.json().risk).toBe("read_only");
    const denied = await app.inject({ method: "POST", url: "/v1/ai/tools/unregistered/invoke", headers });
    expect(denied.statusCode).toBe(404);
    await app.close();
  });
});

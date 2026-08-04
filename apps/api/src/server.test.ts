import { describe, expect, it } from "vitest";
import { buildServer } from "./server";
import type { ApiConfig } from "./config";

const config: ApiConfig = {
  host: "127.0.0.1",
  port: 0,
  nodeEnv: "test",
  sessionSecret: "test-secret-at-least-thirty-two-characters",
  devAuthEnabled: true,
  corsOrigin: "http://localhost:5173",
  storageMode: "memory"
};

async function session(app: ReturnType<typeof buildServer>) {
  const response = await app.inject({ method: "POST", url: "/v1/auth/dev-session" });
  expect(response.statusCode).toBe(200);
  return response.json() as { token: string; sessionId: string; organizationId: string };
}

function headers(auth: { token: string; organizationId: string }) {
  return {
    authorization: `Bearer ${auth.token}`,
    "x-organization-id": auth.organizationId
  };
}

describe("PeopleSyncD API", () => {
  it("serves health checks", async () => {
    const app = buildServer(config);
    const response = await app.inject({ method: "GET", url: "/health/ready" });
    expect(response.statusCode).toBe(200);
    expect(response.json().certified).toBe(false);
    expect(response.json().identityStorage).toBe("in-memory");
    await app.close();
  });

  it("returns the authoritative board on Founder Dashboard", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const response = await app.inject({ method: "GET", url: "/v1/founder/dashboard", headers: headers(auth) });
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
    const requestHeaders = headers(auth);
    const created = await app.inject({
      method: "POST",
      url: "/v1/persons",
      headers: requestHeaders,
      payload: { displayName: "Taylor Reed", preferredName: "Taylor" }
    });
    expect(created.statusCode).toBe(201);
    const audit = await app.inject({ method: "GET", url: "/v1/audit/events", headers: requestHeaders });
    expect(audit.json().some((event: { action: string }) => event.action === "person.create")).toBe(true);
    await app.close();
  });

  it("invokes only the registered read-only Founder Brief tool", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const requestHeaders = headers(auth);
    const response = await app.inject({ method: "POST", url: "/v1/ai/tools/founder.get_brief/invoke", headers: requestHeaders });
    expect(response.statusCode).toBe(200);
    expect(response.json().risk).toBe("read_only");
    const denied = await app.inject({ method: "POST", url: "/v1/ai/tools/unregistered/invoke", headers: requestHeaders });
    expect(denied.statusCode).toBe(404);
    await app.close();
  });

  it("revokes a session immediately on logout", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const requestHeaders = headers(auth);
    const logout = await app.inject({ method: "POST", url: "/v1/auth/logout", headers: requestHeaders });
    expect(logout.statusCode).toBe(204);
    const denied = await app.inject({ method: "GET", url: "/v1/founder/dashboard", headers: requestHeaders });
    expect(denied.statusCode).toBe(401);
    await app.close();
  });

  it("creates pending MFA enrollment records without storing a credential in the response", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const requestHeaders = headers(auth);
    const enrolled = await app.inject({
      method: "POST",
      url: "/v1/auth/mfa/methods",
      headers: requestHeaders,
      payload: { method: "totp", label: "Founder authenticator" }
    });
    expect(enrolled.statusCode).toBe(201);
    expect(enrolled.json().status).toBe("pending");
    expect(enrolled.body).not.toContain("secret");
    const listed = await app.inject({ method: "GET", url: "/v1/auth/mfa/methods", headers: requestHeaders });
    expect(listed.json()).toHaveLength(1);
    await app.close();
  });

  it("returns persisted organization membership authority", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const response = await app.inject({ method: "GET", url: "/v1/organizations/memberships", headers: headers(auth) });
    expect(response.statusCode).toBe(200);
    expect(response.json()[0].displayName).toBe("Jason Henderson");
    expect(response.json()[0].roleKey).toBe("founder");
    await app.close();
  });
});

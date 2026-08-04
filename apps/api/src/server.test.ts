import { describe, expect, it } from "vitest";
import { totpCode } from "@peoplesyncd/auth";
import { buildServer } from "./server";
import type { ApiConfig } from "./config";

const config: ApiConfig = {
  host: "127.0.0.1",
  port: 0,
  nodeEnv: "test",
  sessionSecret: "test-secret-at-least-thirty-two-characters",
  mfaEncryptionKey: "test-mfa-key-at-least-thirty-two-characters",
  devAuthEnabled: true,
  corsOrigin: "http://localhost:5173",
  storageMode: "memory",
  databaseRoleMode: "assume"
};

async function session(app: ReturnType<typeof buildServer>) {
  const response = await app.inject({ method: "POST", url: "/v1/auth/dev-session" });
  expect(response.statusCode).toBe(200);
  return response.json() as { token: string; sessionId: string; sessionFamilyId: string; organizationId: string };
}

function headers(auth: { token: string; organizationId: string }) {
  return {
    authorization: `Bearer ${auth.token}`,
    "x-organization-id": auth.organizationId
  };
}

async function enrollTotp(app: ReturnType<typeof buildServer>, auth: { token: string; organizationId: string }) {
  const response = await app.inject({
    method: "POST",
    url: "/v1/auth/mfa/methods",
    headers: headers(auth),
    payload: { method: "totp", label: "Founder authenticator" }
  });
  expect(response.statusCode).toBe(201);
  const provisioningUri = new URL(response.json().provisioningUri as string);
  const secret = provisioningUri.searchParams.get("secret");
  expect(secret).toBeTruthy();
  return { methodId: response.json().method.id as string, secret: secret as string };
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

  it("rotates a session while preserving its family and invalidating the old token", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const rotated = await app.inject({ method: "POST", url: "/v1/auth/session/rotate", headers: headers(auth) });
    expect(rotated.statusCode).toBe(200);
    expect(rotated.json().sessionId).not.toBe(auth.sessionId);
    expect(rotated.json().sessionFamilyId).toBe(auth.sessionFamilyId);
    const denied = await app.inject({ method: "GET", url: "/v1/founder/dashboard", headers: headers(auth) });
    expect(denied.statusCode).toBe(401);
    const accepted = await app.inject({
      method: "GET",
      url: "/v1/founder/dashboard",
      headers: headers({ token: rotated.json().token, organizationId: auth.organizationId })
    });
    expect(accepted.statusCode).toBe(200);
    await app.close();
  });

  it("completes TOTP enrollment and issues one-time recovery codes", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const enrolled = await enrollTotp(app, auth);
    const verified = await app.inject({
      method: "POST",
      url: `/v1/auth/mfa/totp/${enrolled.methodId}/verify`,
      headers: headers(auth),
      payload: { code: totpCode(enrolled.secret) }
    });
    expect(verified.statusCode).toBe(200);
    expect(verified.json().method.status).toBe("active");
    expect(verified.json().authenticationMethods).toContain("totp");
    expect(verified.json().recoveryCodes).toHaveLength(8);
    const oldDenied = await app.inject({ method: "GET", url: "/v1/founder/dashboard", headers: headers(auth) });
    expect(oldDenied.statusCode).toBe(401);
    await app.close();
  });

  it("rejects reuse of the same TOTP time-step counter", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const enrolled = await enrollTotp(app, auth);
    const code = totpCode(enrolled.secret);
    const verified = await app.inject({
      method: "POST",
      url: `/v1/auth/mfa/totp/${enrolled.methodId}/verify`,
      headers: headers(auth),
      payload: { code }
    });
    expect(verified.statusCode).toBe(200);
    const replay = await app.inject({
      method: "POST",
      url: `/v1/auth/mfa/totp/${enrolled.methodId}/verify`,
      headers: headers({ token: verified.json().token, organizationId: auth.organizationId }),
      payload: { code }
    });
    expect(replay.statusCode).toBe(401);
    expect(replay.json().error).toMatch(/replay/i);
    await app.close();
  });

  it("consumes each recovery code once and rotates the session", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const enrolled = await enrollTotp(app, auth);
    const verified = await app.inject({
      method: "POST",
      url: `/v1/auth/mfa/totp/${enrolled.methodId}/verify`,
      headers: headers(auth),
      payload: { code: totpCode(enrolled.secret) }
    });
    const recoveryCode = verified.json().recoveryCodes[0] as string;
    const recovered = await app.inject({
      method: "POST",
      url: "/v1/auth/mfa/recovery/consume",
      headers: headers({ token: verified.json().token, organizationId: auth.organizationId }),
      payload: { code: recoveryCode }
    });
    expect(recovered.statusCode).toBe(200);
    expect(recovered.json().authenticationMethods).toContain("recovery_code");
    expect(recovered.json().remainingCodes).toBe(7);
    const reused = await app.inject({
      method: "POST",
      url: "/v1/auth/mfa/recovery/consume",
      headers: headers({ token: recovered.json().token, organizationId: auth.organizationId }),
      payload: { code: recoveryCode }
    });
    expect(reused.statusCode).toBe(401);
    await app.close();
  });

  it("rejects an invalid TOTP verification code without activating the method", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const enrolled = await enrollTotp(app, auth);
    const validCode = totpCode(enrolled.secret);
    const invalidCode = `${validCode[0] === "0" ? "1" : "0"}${validCode.slice(1)}`;
    const denied = await app.inject({
      method: "POST",
      url: `/v1/auth/mfa/totp/${enrolled.methodId}/verify`,
      headers: headers(auth),
      payload: { code: invalidCode }
    });
    expect(denied.statusCode).toBe(401);
    const listed = await app.inject({ method: "GET", url: "/v1/auth/mfa/methods", headers: headers(auth) });
    expect(listed.json()[0].status).toBe("pending");
    await app.close();
  });

  it("prevents suspending or de-authorizing the final active Founder", async () => {
    const app = buildServer(config);
    const auth = await session(app);
    const response = await app.inject({
      method: "PATCH",
      url: "/v1/organizations/memberships/founder-jason",
      headers: headers(auth),
      payload: { status: "suspended", permissions: ["organization.membership.read"] }
    });
    expect(response.statusCode).toBe(409);
    const stillAccepted = await app.inject({ method: "GET", url: "/v1/founder/dashboard", headers: headers(auth) });
    expect(stillAccepted.statusCode).toBe(200);
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

import { describe, expect, it } from "vitest";
import { createSessionToken, verifySessionToken } from "./index";

const secret = "development-secret-at-least-16";

describe("session tokens", () => {
  it("round trips valid claims", () => {
    const token = createSessionToken({
      sessionId: "session-1",
      subject: "founder",
      displayName: "Founder",
      organizationIds: ["org"],
      permissions: ["founder.dashboard.read"],
      authenticationMethods: ["development"],
      issuedAt: 100,
      expiresAt: 200
    }, secret);
    const claims = verifySessionToken(token, secret, 150);
    expect(claims.subject).toBe("founder");
    expect(claims.sessionId).toBe("session-1");
  });

  it("rejects tampering", () => {
    const token = createSessionToken({
      sessionId: "session-2",
      subject: "founder",
      displayName: "Founder",
      organizationIds: ["org"],
      permissions: [],
      authenticationMethods: ["development"],
      issuedAt: 100,
      expiresAt: 200
    }, secret);
    expect(() => verifySessionToken(`${token}x`, secret, 150)).toThrow();
  });
});

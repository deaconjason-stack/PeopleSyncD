import { describe, expect, it } from "vitest";
import { createSessionToken, verifySessionToken } from "./index";

const secret = "development-secret-at-least-16";

describe("session tokens", () => {
  it("round trips valid claims", () => {
    const token = createSessionToken({
      subject: "founder",
      displayName: "Founder",
      organizationIds: ["org"],
      permissions: ["founder.dashboard.read"],
      issuedAt: 100,
      expiresAt: 200
    }, secret);
    expect(verifySessionToken(token, secret, 150).subject).toBe("founder");
  });

  it("rejects tampering", () => {
    const token = createSessionToken({
      subject: "founder",
      displayName: "Founder",
      organizationIds: ["org"],
      permissions: [],
      issuedAt: 100,
      expiresAt: 200
    }, secret);
    expect(() => verifySessionToken(`${token}x`, secret, 150)).toThrow();
  });
});

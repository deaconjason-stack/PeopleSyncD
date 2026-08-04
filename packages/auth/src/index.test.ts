import { describe, expect, it } from "vitest";
import {
  createSessionToken,
  decryptCredential,
  encryptCredential,
  totpCode,
  verifySessionToken,
  verifyTotpCode
} from "./index";

const secret = "development-secret-at-least-16";
const credentialKey = "credential-key-material-at-least-thirty-two";

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

describe("TOTP and credential protection", () => {
  it("matches the RFC 6238 SHA-1 test vector", () => {
    const rfcSecret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";
    expect(totpCode(rfcSecret, 59_000, 30, 8)).toBe("94287082");
    expect(verifyTotpCode(rfcSecret, "94287082", 59_000, 0, 30, 8)).toBe(true);
  });

  it("encrypts authenticator credentials with authenticated encryption", () => {
    const encrypted = encryptCredential("TOP-SECRET", credentialKey);
    expect(encrypted).not.toContain("TOP-SECRET");
    expect(decryptCredential(encrypted, credentialKey)).toBe("TOP-SECRET");
    expect(() => decryptCredential(encrypted, `${credentialKey}-wrong`)).toThrow();
  });
});

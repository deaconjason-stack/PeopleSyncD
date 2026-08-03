import { describe, expect, it } from "vitest";
import { authorize } from "./index";
import type { SessionClaims } from "@peoplesyncd/shared";

const claims: SessionClaims = {
  subject: "user",
  displayName: "User",
  organizationIds: ["org-a"],
  permissions: ["person.read.summary"],
  issuedAt: 1,
  expiresAt: 9999999999
};

describe("authorization", () => {
  it("allows matching tenant and permission", () => {
    expect(authorize(claims, "org-a", "person.read.summary")).toBe("org-a");
  });

  it("rejects cross-tenant access", () => {
    expect(() => authorize(claims, "org-b", "person.read.summary")).toThrow("not authorized");
  });
});

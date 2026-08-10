import { describe, expect, it } from "vitest";
import { readConfig } from "./config";

const productionSecrets = {
  NODE_ENV: "production",
  PEOPLESYNCD_SESSION_SECRET: "production-session-secret-at-least-thirty-two-characters",
  PEOPLESYNCD_MFA_ENCRYPTION_KEY: "production-mfa-encryption-key-at-least-thirty-two-characters",
  PEOPLESYNCD_STORAGE: "postgres"
};

describe("PeopleSyncD API configuration", () => {
  it("requires a dedicated runtime database URL in production", () => {
    expect(() => readConfig({
      ...productionSecrets,
      PEOPLESYNCD_DATABASE_URL: "postgresql://migration-owner@example.invalid/peoplesyncd"
    })).toThrow(/RUNTIME_DATABASE_URL/);
  });

  it("requires direct runtime database identity in production", () => {
    expect(() => readConfig({
      ...productionSecrets,
      PEOPLESYNCD_RUNTIME_DATABASE_URL: "postgresql://runtime@example.invalid/peoplesyncd",
      PEOPLESYNCD_DATABASE_ROLE_MODE: "assume"
    })).toThrow(/Direct runtime database identity/);
  });

  it("accepts a dedicated direct runtime identity in production", () => {
    const config = readConfig({
      ...productionSecrets,
      PEOPLESYNCD_RUNTIME_DATABASE_URL: "postgresql://runtime@example.invalid/peoplesyncd",
      PEOPLESYNCD_DATABASE_ROLE_MODE: "direct"
    });
    expect(config.databaseRoleMode).toBe("direct");
    expect(config.databaseUrl).toContain("runtime@");
    expect(config.devAuthEnabled).toBe(false);
  });

  it("retains assume-role mode for controlled development and CI", () => {
    const config = readConfig({
      NODE_ENV: "test",
      PEOPLESYNCD_SESSION_SECRET: "test-session-secret-at-least-thirty-two-characters",
      PEOPLESYNCD_MFA_ENCRYPTION_KEY: "test-mfa-encryption-key-at-least-thirty-two-characters",
      PEOPLESYNCD_DATABASE_URL: "postgresql://owner@example.invalid/peoplesyncd",
      PEOPLESYNCD_STORAGE: "postgres"
    });
    expect(config.databaseRoleMode).toBe("assume");
  });
});

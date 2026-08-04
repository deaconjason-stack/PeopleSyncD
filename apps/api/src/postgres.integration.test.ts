import { randomUUID } from "node:crypto";
import { Pool } from "pg";
import { afterAll, beforeAll, describe, expect, it } from "vitest";
import { totpCode } from "@peoplesyncd/auth";
import { GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import { PostgresIdentityStore } from "./identity";
import { PostgresPlatformStore } from "./store";

const databaseUrl = process.env.PEOPLESYNCD_TEST_DATABASE_URL;
const describePostgres = databaseUrl ? describe : describe.skip;
const secondaryOrganizationId = "99999999-9999-4999-8999-999999999999";
const mfaEncryptionKey = "integration-mfa-key-at-least-thirty-two-characters";

describePostgres("PostgreSQL platform and identity repositories", () => {
  let ownerPool: Pool;
  let store: PostgresPlatformStore;
  let identity: PostgresIdentityStore;

  beforeAll(async () => {
    if (!databaseUrl) throw new Error("PEOPLESYNCD_TEST_DATABASE_URL is required");
    ownerPool = new Pool({ connectionString: databaseUrl });
    store = new PostgresPlatformStore(databaseUrl);
    identity = new PostgresIdentityStore(databaseUrl, mfaEncryptionKey);
    await ownerPool.query(
      `INSERT INTO organizations (id, name)
       VALUES ($1, 'Tenant Isolation Test Organization')
       ON CONFLICT (id) DO NOTHING`,
      [secondaryOrganizationId]
    );
  });

  afterAll(async () => {
    if (ownerPool) {
      await ownerPool.query("DELETE FROM workers WHERE organization_id = $1", [secondaryOrganizationId]);
      await ownerPool.query("DELETE FROM persons WHERE organization_id = $1", [secondaryOrganizationId]);
      await ownerPool.query("DELETE FROM organizations WHERE id = $1", [secondaryOrganizationId]);
      await ownerPool.end();
    }
    if (store) await store.close();
    if (identity) await identity.close();
  });

  it("persists person records and prevents cross-tenant leakage", async () => {
    const displayName = `Tenant Test ${randomUUID()}`;
    const created = await store.createPerson(secondaryOrganizationId, { displayName });
    expect(created.organizationId).toBe(secondaryOrganizationId);

    const secondaryPeople = await store.listPersons(secondaryOrganizationId);
    expect(secondaryPeople.some((person) => person.id === created.id)).toBe(true);

    const genesisPeople = await store.listPersons(GENESIS_ORGANIZATION_ID);
    expect(genesisPeople.some((person) => person.id === created.id)).toBe(false);
  });

  it("reports PostgreSQL readiness", async () => {
    await expect(store.health()).resolves.toBe(true);
    await expect(identity.health()).resolves.toBe(true);
  });

  it("persists and immediately revokes server-side sessions", async () => {
    const membership = await identity.getMembership("founder-jason", GENESIS_ORGANIZATION_ID);
    expect(membership.roleKey).toBe("founder");
    const session = await identity.createSession({
      organizationId: GENESIS_ORGANIZATION_ID,
      userId: membership.userId,
      authenticationMethods: ["development"],
      expiresAt: new Date(Date.now() + 60_000).toISOString(),
      correlationId: randomUUID()
    });
    await expect(identity.validateSession(session.id, membership.userId, GENESIS_ORGANIZATION_ID)).resolves.toBe(true);
    await identity.revokeSession(
      GENESIS_ORGANIZATION_ID,
      session.id,
      membership.userId,
      membership.userId,
      randomUUID()
    );
    await expect(identity.validateSession(session.id, membership.userId, GENESIS_ORGANIZATION_ID)).resolves.toBe(false);
  });

  it("rotates sessions atomically within one session family", async () => {
    const original = await identity.createSession({
      organizationId: GENESIS_ORGANIZATION_ID,
      userId: "founder-jason",
      authenticationMethods: ["development"],
      expiresAt: new Date(Date.now() + 60_000).toISOString(),
      correlationId: randomUUID()
    });
    const replacement = await identity.rotateSession(
      GENESIS_ORGANIZATION_ID,
      original.id,
      "founder-jason",
      ["development"],
      new Date(Date.now() + 120_000).toISOString(),
      randomUUID()
    );
    expect(replacement.id).not.toBe(original.id);
    expect(replacement.sessionFamilyId).toBe(original.sessionFamilyId);
    expect(replacement.rotatedFrom).toBe(original.id);
    await expect(identity.validateSession(original.id, "founder-jason", GENESIS_ORGANIZATION_ID)).resolves.toBe(false);
    await expect(identity.validateSession(replacement.id, "founder-jason", GENESIS_ORGANIZATION_ID)).resolves.toBe(true);
  });

  it("persists pending WebAuthn enrollment records", async () => {
    const enrolled = await identity.enrollMfaMethod(
      GENESIS_ORGANIZATION_ID,
      "founder-jason",
      "webauthn",
      "Founder security key",
      randomUUID()
    );
    expect(enrolled.status).toBe("pending");
    const listed = await identity.listMfaMethods(GENESIS_ORGANIZATION_ID, "founder-jason");
    expect(listed.some((method) => method.id === enrolled.id)).toBe(true);
  });

  it("activates encrypted TOTP enrollment and rotates the authenticated session", async () => {
    const session = await identity.createSession({
      organizationId: GENESIS_ORGANIZATION_ID,
      userId: "founder-jason",
      authenticationMethods: ["development"],
      expiresAt: new Date(Date.now() + 60_000).toISOString(),
      correlationId: randomUUID()
    });
    const enrollment = await identity.enrollTotpMethod(
      GENESIS_ORGANIZATION_ID,
      "founder-jason",
      "Founder authenticator",
      "deaconjason@medisyncdtechnologies.com",
      "PeopleSyncD Genesis",
      randomUUID()
    );
    const provisioningUri = new URL(enrollment.provisioningUri);
    const secret = provisioningUri.searchParams.get("secret");
    expect(secret).toBeTruthy();
    const verified = await identity.verifyTotpMethod(
      GENESIS_ORGANIZATION_ID,
      "founder-jason",
      enrollment.method.id,
      totpCode(secret as string),
      session.id,
      new Date(Date.now() + 120_000).toISOString(),
      randomUUID()
    );
    expect(verified.method.status).toBe("active");
    expect(verified.session.authenticationMethods).toContain("totp");
    expect(verified.recoveryCodes).toHaveLength(8);
    await expect(identity.validateSession(session.id, "founder-jason", GENESIS_ORGANIZATION_ID)).resolves.toBe(false);
    await expect(identity.validateSession(verified.session.id, "founder-jason", GENESIS_ORGANIZATION_ID)).resolves.toBe(true);
  });
});

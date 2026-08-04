import { randomUUID } from "node:crypto";
import { Pool } from "pg";
import { afterAll, beforeAll, describe, expect, it } from "vitest";
import { GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import { PostgresIdentityStore } from "./identity";
import { PostgresPlatformStore } from "./store";

const databaseUrl = process.env.PEOPLESYNCD_TEST_DATABASE_URL;
const describePostgres = databaseUrl ? describe : describe.skip;
const secondaryOrganizationId = "99999999-9999-4999-8999-999999999999";

describePostgres("PostgreSQL platform and identity repositories", () => {
  let ownerPool: Pool;
  let store: PostgresPlatformStore;
  let identity: PostgresIdentityStore;

  beforeAll(async () => {
    if (!databaseUrl) throw new Error("PEOPLESYNCD_TEST_DATABASE_URL is required");
    ownerPool = new Pool({ connectionString: databaseUrl });
    store = new PostgresPlatformStore(databaseUrl);
    identity = new PostgresIdentityStore(databaseUrl);
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

  it("persists pending MFA enrollment records", async () => {
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
});

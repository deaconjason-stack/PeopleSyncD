import { randomUUID } from "node:crypto";
import { Pool } from "pg";
import { afterAll, beforeAll, describe, expect, it } from "vitest";
import { GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import { PostgresPlatformStore } from "./store";

const databaseUrl = process.env.PEOPLESYNCD_TEST_DATABASE_URL;
const describePostgres = databaseUrl ? describe : describe.skip;
const secondaryOrganizationId = "99999999-9999-4999-8999-999999999999";

describePostgres("PostgreSQL platform repository", () => {
  let ownerPool: Pool;
  let store: PostgresPlatformStore;

  beforeAll(async () => {
    if (!databaseUrl) throw new Error("PEOPLESYNCD_TEST_DATABASE_URL is required");
    ownerPool = new Pool({ connectionString: databaseUrl });
    store = new PostgresPlatformStore(databaseUrl);
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
  });
});

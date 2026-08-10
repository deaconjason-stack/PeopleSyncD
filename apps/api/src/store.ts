import { randomUUID } from "node:crypto";
import { Pool, type PoolClient } from "pg";
import { ACTIVE_BOARD, GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import type { AuditEvent, FounderDashboard, PersonSummary, WorkerSummary } from "@peoplesyncd/shared";
import { InMemoryAuditStore } from "@peoplesyncd/audit";
import type { ApiConfig } from "./config";

export type AuditAppendInput = Omit<AuditEvent, "id" | "occurredAt">;

export interface AuditRepository {
  append(input: AuditAppendInput): Promise<AuditEvent>;
  list(organizationId: string, limit?: number): Promise<AuditEvent[]>;
}

export interface PlatformStore {
  readonly kind: "in-memory" | "postgres";
  readonly audit: AuditRepository;
  health(): Promise<boolean>;
  listPersons(organizationId: string): Promise<PersonSummary[]>;
  createPerson(organizationId: string, input: { displayName: string; preferredName?: string }): Promise<PersonSummary>;
  listWorkers(organizationId: string): Promise<WorkerSummary[]>;
  createWorker(
    organizationId: string,
    input: Omit<WorkerSummary, "id" | "organizationId" | "rowVersion">
  ): Promise<WorkerSummary>;
  dashboard(organizationId: string): Promise<FounderDashboard>;
  close(): Promise<void>;
}

export class InMemoryPlatformStore implements PlatformStore {
  readonly kind = "in-memory" as const;
  readonly audit: AuditRepository;
  private readonly auditStore = new InMemoryAuditStore();
  private readonly persons: PersonSummary[] = [
    {
      id: "22222222-2222-4222-8222-222222222222",
      organizationId: GENESIS_ORGANIZATION_ID,
      displayName: "Alex Morgan",
      preferredName: "Alex",
      createdAt: new Date("2026-08-03T12:00:00Z").toISOString()
    }
  ];
  private readonly workers: WorkerSummary[] = [
    {
      id: "33333333-3333-4333-8333-333333333333",
      organizationId: GENESIS_ORGANIZATION_ID,
      personId: "22222222-2222-4222-8222-222222222222",
      workerType: "employee",
      employmentStatus: "onboarding",
      startDate: "2026-08-10",
      rowVersion: 1
    }
  ];

  constructor() {
    this.audit = {
      append: async (input) => this.auditStore.append(input),
      list: async (organizationId, limit = 20) => this.auditStore.list(organizationId, limit)
    };
  }

  async health(): Promise<boolean> {
    return true;
  }

  async listPersons(organizationId: string): Promise<PersonSummary[]> {
    return this.persons.filter((person) => person.organizationId === organizationId);
  }

  async createPerson(
    organizationId: string,
    input: { displayName: string; preferredName?: string }
  ): Promise<PersonSummary> {
    const person: PersonSummary = {
      id: randomUUID(),
      organizationId,
      displayName: input.displayName.trim(),
      preferredName: input.preferredName?.trim() || undefined,
      createdAt: new Date().toISOString()
    };
    this.persons.push(person);
    return person;
  }

  async listWorkers(organizationId: string): Promise<WorkerSummary[]> {
    return this.workers.filter((worker) => worker.organizationId === organizationId);
  }

  async createWorker(
    organizationId: string,
    input: Omit<WorkerSummary, "id" | "organizationId" | "rowVersion">
  ): Promise<WorkerSummary> {
    if (!this.persons.some((person) => person.id === input.personId && person.organizationId === organizationId)) {
      throw new Error("Person not found in organization");
    }
    const worker: WorkerSummary = {
      id: randomUUID(),
      organizationId,
      ...input,
      rowVersion: 1
    };
    this.workers.push(worker);
    return worker;
  }

  async dashboard(organizationId: string): Promise<FounderDashboard> {
    const workers = await this.listWorkers(organizationId);
    const persons = await this.listPersons(organizationId);
    return {
      organizationId,
      activeWorkers: workers.filter((worker) => worker.employmentStatus === "active").length,
      onboardingWorkers: workers.filter((worker) => worker.employmentStatus === "onboarding").length,
      people: persons.length,
      pendingApprovals: 0,
      board: ACTIVE_BOARD,
      recentAudit: await this.audit.list(organizationId, 10),
      generatedAt: new Date().toISOString()
    };
  }

  async close(): Promise<void> {
    return undefined;
  }
}

interface PersonRow {
  id: string;
  organization_id: string;
  display_name: string;
  preferred_name: string | null;
  created_at: Date | string;
}

interface WorkerRow {
  id: string;
  organization_id: string;
  person_id: string;
  worker_type: WorkerSummary["workerType"];
  employment_status: WorkerSummary["employmentStatus"];
  start_date: Date | string;
  row_version: string | number;
}

interface AuditRow {
  id: string;
  organization_id: string;
  actor_id: string;
  action: string;
  resource_type: string;
  resource_id: string | null;
  outcome: AuditEvent["outcome"];
  occurred_at: Date | string;
  correlation_id: string;
  metadata: AuditEvent["metadata"] | null;
}

function requireFirst<T>(rows: T[], message: string): T {
  const row = rows[0];
  if (!row) throw new Error(message);
  return row;
}

function iso(value: Date | string): string {
  return value instanceof Date ? value.toISOString() : new Date(value).toISOString();
}

function dateOnly(value: Date | string): string {
  if (typeof value === "string") return value.slice(0, 10);
  return value.toISOString().slice(0, 10);
}

function mapPerson(row: PersonRow): PersonSummary {
  return {
    id: row.id,
    organizationId: row.organization_id,
    displayName: row.display_name,
    preferredName: row.preferred_name ?? undefined,
    createdAt: iso(row.created_at)
  };
}

function mapWorker(row: WorkerRow): WorkerSummary {
  return {
    id: row.id,
    organizationId: row.organization_id,
    personId: row.person_id,
    workerType: row.worker_type,
    employmentStatus: row.employment_status,
    startDate: dateOnly(row.start_date),
    rowVersion: Number(row.row_version)
  };
}

function mapAudit(row: AuditRow): AuditEvent {
  return {
    id: row.id,
    organizationId: row.organization_id,
    actorId: row.actor_id,
    action: row.action,
    resourceType: row.resource_type,
    resourceId: row.resource_id ?? undefined,
    outcome: row.outcome,
    occurredAt: iso(row.occurred_at),
    correlationId: row.correlation_id,
    metadata: row.metadata ?? undefined
  };
}

export class PostgresPlatformStore implements PlatformStore {
  readonly kind = "postgres" as const;
  readonly audit: AuditRepository;
  private readonly pool: Pool;

  constructor(databaseUrl: string) {
    this.pool = new Pool({
      connectionString: databaseUrl,
      max: 10,
      idleTimeoutMillis: 30_000,
      connectionTimeoutMillis: 5_000,
      statement_timeout: 10_000,
      application_name: "peoplesyncd-api"
    });
    this.audit = {
      append: async (input) => this.appendAudit(input),
      list: async (organizationId, limit = 20) => this.listAudit(organizationId, limit)
    };
  }

  private async withTenant<T>(organizationId: string, operation: (client: PoolClient) => Promise<T>): Promise<T> {
    const client = await this.pool.connect();
    try {
      await client.query("BEGIN");
      await client.query("SET LOCAL ROLE peoplesyncd_runtime");
      await client.query("SELECT set_config('app.organization_id', $1, true)", [organizationId]);
      const result = await operation(client);
      await client.query("COMMIT");
      return result;
    } catch (error) {
      await client.query("ROLLBACK").catch(() => undefined);
      throw error;
    } finally {
      client.release();
    }
  }

  async health(): Promise<boolean> {
    try {
      await this.pool.query("SELECT 1");
      return true;
    } catch {
      return false;
    }
  }

  async listPersons(organizationId: string): Promise<PersonSummary[]> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT id, organization_id, display_name, preferred_name, created_at
         FROM persons
         WHERE archived_at IS NULL
         ORDER BY display_name, id`
      );
      return (result.rows as PersonRow[]).map(mapPerson);
    });
  }

  async createPerson(
    organizationId: string,
    input: { displayName: string; preferredName?: string }
  ): Promise<PersonSummary> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `INSERT INTO persons (id, organization_id, display_name, preferred_name)
         VALUES ($1, $2, $3, $4)
         RETURNING id, organization_id, display_name, preferred_name, created_at`,
        [randomUUID(), organizationId, input.displayName.trim(), input.preferredName?.trim() || null]
      );
      return mapPerson(requireFirst(result.rows as PersonRow[], "Person insert returned no row"));
    });
  }

  async listWorkers(organizationId: string): Promise<WorkerSummary[]> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT id, organization_id, person_id, worker_type, employment_status, start_date, row_version
         FROM workers
         ORDER BY start_date, id`
      );
      return (result.rows as WorkerRow[]).map(mapWorker);
    });
  }

  async createWorker(
    organizationId: string,
    input: Omit<WorkerSummary, "id" | "organizationId" | "rowVersion">
  ): Promise<WorkerSummary> {
    return this.withTenant(organizationId, async (client) => {
      const person = await client.query("SELECT id FROM persons WHERE id = $1 AND archived_at IS NULL", [input.personId]);
      if (person.rowCount !== 1) throw new Error("Person not found in organization");
      const result = await client.query(
        `INSERT INTO workers (id, organization_id, person_id, worker_type, employment_status, start_date)
         VALUES ($1, $2, $3, $4, $5, $6)
         RETURNING id, organization_id, person_id, worker_type, employment_status, start_date, row_version`,
        [randomUUID(), organizationId, input.personId, input.workerType, input.employmentStatus, input.startDate]
      );
      return mapWorker(requireFirst(result.rows as WorkerRow[], "Worker insert returned no row"));
    });
  }

  private async appendAudit(input: AuditAppendInput): Promise<AuditEvent> {
    return this.withTenant(input.organizationId, async (client) => {
      const result = await client.query(
        `INSERT INTO audit_events
           (id, organization_id, actor_id, action, resource_type, resource_id, outcome, correlation_id, metadata)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9::jsonb)
         RETURNING id, organization_id, actor_id, action, resource_type, resource_id,
                   outcome, correlation_id, metadata, occurred_at`,
        [
          randomUUID(),
          input.organizationId,
          input.actorId,
          input.action,
          input.resourceType,
          input.resourceId ?? null,
          input.outcome,
          input.correlationId,
          JSON.stringify(input.metadata ?? {})
        ]
      );
      return mapAudit(requireFirst(result.rows as AuditRow[], "Audit insert returned no row"));
    });
  }

  private async listAudit(organizationId: string, limit: number): Promise<AuditEvent[]> {
    const safeLimit = Math.min(Math.max(Math.trunc(limit), 1), 100);
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT id, organization_id, actor_id, action, resource_type, resource_id,
                outcome, correlation_id, metadata, occurred_at
         FROM audit_events
         ORDER BY occurred_at DESC, id DESC
         LIMIT $1`,
        [safeLimit]
      );
      return (result.rows as AuditRow[]).map(mapAudit);
    });
  }

  async dashboard(organizationId: string): Promise<FounderDashboard> {
    const [workers, persons, recentAudit] = await Promise.all([
      this.listWorkers(organizationId),
      this.listPersons(organizationId),
      this.audit.list(organizationId, 10)
    ]);
    return {
      organizationId,
      activeWorkers: workers.filter((worker) => worker.employmentStatus === "active").length,
      onboardingWorkers: workers.filter((worker) => worker.employmentStatus === "onboarding").length,
      people: persons.length,
      pendingApprovals: 0,
      board: ACTIVE_BOARD,
      recentAudit,
      generatedAt: new Date().toISOString()
    };
  }

  async close(): Promise<void> {
    await this.pool.end();
  }
}

export function createPlatformStore(config: Pick<ApiConfig, "storageMode" | "databaseUrl">): PlatformStore {
  if (config.storageMode === "postgres") {
    if (!config.databaseUrl) throw new Error("PostgreSQL storage requires a database URL");
    return new PostgresPlatformStore(config.databaseUrl);
  }
  return new InMemoryPlatformStore();
}

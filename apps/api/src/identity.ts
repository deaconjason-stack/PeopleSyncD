import { randomUUID } from "node:crypto";
import { Pool, type PoolClient } from "pg";
import { GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import type {
  MfaMethodSummary,
  OrganizationMembershipSummary,
  Permission,
  SessionSummary
} from "@peoplesyncd/shared";
import type { ApiConfig } from "./config";

export interface CreateSessionInput {
  organizationId: string;
  userId: string;
  authenticationMethods: string[];
  expiresAt: string;
  correlationId: string;
}

export interface IdentityStore {
  readonly kind: "in-memory" | "postgres";
  health(): Promise<boolean>;
  getMembership(userId: string, organizationId: string): Promise<OrganizationMembershipSummary>;
  listMemberships(organizationId: string): Promise<OrganizationMembershipSummary[]>;
  createSession(input: CreateSessionInput): Promise<SessionSummary>;
  validateSession(sessionId: string, userId: string, organizationId: string): Promise<boolean>;
  listSessions(organizationId: string, userId: string): Promise<SessionSummary[]>;
  revokeSession(organizationId: string, sessionId: string, userId: string, revokedBy: string, correlationId: string): Promise<SessionSummary>;
  listMfaMethods(organizationId: string, userId: string): Promise<MfaMethodSummary[]>;
  enrollMfaMethod(
    organizationId: string,
    userId: string,
    method: MfaMethodSummary["method"],
    label: string | undefined,
    correlationId: string
  ): Promise<MfaMethodSummary>;
  close(): Promise<void>;
}

const FOUNDER_PERMISSIONS: Permission[] = [
  "founder.dashboard.read",
  "person.read.summary",
  "person.create",
  "worker.read",
  "worker.create",
  "audit.append",
  "audit.read",
  "ai.tool.founder.get_brief",
  "identity.session.read",
  "identity.session.revoke",
  "identity.mfa.read",
  "identity.mfa.enroll",
  "organization.membership.read"
];

export class InMemoryIdentityStore implements IdentityStore {
  readonly kind = "in-memory" as const;
  private readonly memberships: OrganizationMembershipSummary[] = [
    {
      id: "44444444-4444-4444-8444-444444444444",
      organizationId: GENESIS_ORGANIZATION_ID,
      userId: "founder-jason",
      displayName: "Jason Henderson",
      email: "deaconjason@medisyncdtechnologies.com",
      roleKey: "founder",
      status: "active",
      permissions: FOUNDER_PERMISSIONS,
      createdAt: new Date("2026-08-03T12:00:00Z").toISOString()
    }
  ];
  private readonly sessions: SessionSummary[] = [];
  private readonly mfaMethods: MfaMethodSummary[] = [];

  async health(): Promise<boolean> {
    return true;
  }

  async getMembership(userId: string, organizationId: string): Promise<OrganizationMembershipSummary> {
    const membership = this.memberships.find(
      (candidate) => candidate.userId === userId && candidate.organizationId === organizationId && candidate.status === "active"
    );
    if (!membership) throw new Error("Active membership not found");
    return { ...membership, permissions: [...membership.permissions] };
  }

  async listMemberships(organizationId: string): Promise<OrganizationMembershipSummary[]> {
    return this.memberships
      .filter((membership) => membership.organizationId === organizationId)
      .map((membership) => ({ ...membership, permissions: [...membership.permissions] }));
  }

  async createSession(input: CreateSessionInput): Promise<SessionSummary> {
    await this.getMembership(input.userId, input.organizationId);
    const session: SessionSummary = {
      id: randomUUID(),
      organizationId: input.organizationId,
      userId: input.userId,
      authenticationMethods: [...input.authenticationMethods],
      issuedAt: new Date().toISOString(),
      expiresAt: input.expiresAt
    };
    this.sessions.push(session);
    return { ...session, authenticationMethods: [...session.authenticationMethods] };
  }

  async validateSession(sessionId: string, userId: string, organizationId: string): Promise<boolean> {
    const session = this.sessions.find(
      (candidate) => candidate.id === sessionId && candidate.userId === userId && candidate.organizationId === organizationId
    );
    return Boolean(session && !session.revokedAt && Date.parse(session.expiresAt) > Date.now());
  }

  async listSessions(organizationId: string, userId: string): Promise<SessionSummary[]> {
    return this.sessions
      .filter((session) => session.organizationId === organizationId && session.userId === userId)
      .map((session) => ({ ...session, authenticationMethods: [...session.authenticationMethods] }));
  }

  async revokeSession(
    organizationId: string,
    sessionId: string,
    userId: string,
    revokedBy: string,
    _correlationId: string
  ): Promise<SessionSummary> {
    const session = this.sessions.find(
      (candidate) => candidate.id === sessionId && candidate.organizationId === organizationId && candidate.userId === userId
    );
    if (!session) throw new Error("Session not found");
    session.revokedAt ??= new Date().toISOString();
    session.revokedBy ??= revokedBy;
    return { ...session, authenticationMethods: [...session.authenticationMethods] };
  }

  async listMfaMethods(organizationId: string, userId: string): Promise<MfaMethodSummary[]> {
    return this.mfaMethods
      .filter((method) => method.organizationId === organizationId && method.userId === userId)
      .map((method) => ({ ...method }));
  }

  async enrollMfaMethod(
    organizationId: string,
    userId: string,
    method: MfaMethodSummary["method"],
    label: string | undefined,
    _correlationId: string
  ): Promise<MfaMethodSummary> {
    await this.getMembership(userId, organizationId);
    const record: MfaMethodSummary = {
      id: randomUUID(),
      organizationId,
      userId,
      method,
      label: label?.trim() || undefined,
      status: "pending",
      createdAt: new Date().toISOString()
    };
    this.mfaMethods.push(record);
    return { ...record };
  }

  async close(): Promise<void> {
    return undefined;
  }
}

interface MembershipRow {
  id: string;
  organization_id: string;
  user_id: string;
  display_name: string;
  email: string | null;
  role_key: string;
  status: OrganizationMembershipSummary["status"];
  permissions: Permission[];
  created_at: Date | string;
}

interface SessionRow {
  id: string;
  organization_id: string;
  user_id: string;
  authentication_methods: string[];
  issued_at: Date | string;
  expires_at: Date | string;
  revoked_at: Date | string | null;
  revoked_by: string | null;
}

interface MfaRow {
  id: string;
  organization_id: string;
  user_id: string;
  method: MfaMethodSummary["method"];
  label: string | null;
  status: MfaMethodSummary["status"];
  created_at: Date | string;
  verified_at: Date | string | null;
}

function requireFirst<T>(rows: T[], message: string): T {
  const first = rows[0];
  if (!first) throw new Error(message);
  return first;
}

function iso(value: Date | string): string {
  return value instanceof Date ? value.toISOString() : new Date(value).toISOString();
}

function mapMembership(row: MembershipRow): OrganizationMembershipSummary {
  return {
    id: row.id,
    organizationId: row.organization_id,
    userId: row.user_id,
    displayName: row.display_name,
    email: row.email ?? undefined,
    roleKey: row.role_key,
    status: row.status,
    permissions: row.permissions,
    createdAt: iso(row.created_at)
  };
}

function mapSession(row: SessionRow): SessionSummary {
  return {
    id: row.id,
    organizationId: row.organization_id,
    userId: row.user_id,
    authenticationMethods: row.authentication_methods,
    issuedAt: iso(row.issued_at),
    expiresAt: iso(row.expires_at),
    revokedAt: row.revoked_at ? iso(row.revoked_at) : undefined,
    revokedBy: row.revoked_by ?? undefined
  };
}

function mapMfa(row: MfaRow): MfaMethodSummary {
  return {
    id: row.id,
    organizationId: row.organization_id,
    userId: row.user_id,
    method: row.method,
    label: row.label ?? undefined,
    status: row.status,
    createdAt: iso(row.created_at),
    verifiedAt: row.verified_at ? iso(row.verified_at) : undefined
  };
}

export class PostgresIdentityStore implements IdentityStore {
  readonly kind = "postgres" as const;
  private readonly pool: Pool;

  constructor(databaseUrl: string) {
    this.pool = new Pool({
      connectionString: databaseUrl,
      max: 5,
      idleTimeoutMillis: 30_000,
      connectionTimeoutMillis: 5_000,
      statement_timeout: 10_000,
      application_name: "peoplesyncd-identity"
    });
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

  private async appendSecurityEvent(
    client: PoolClient,
    input: {
      organizationId: string;
      userId: string;
      eventType: string;
      outcome: "success" | "denied" | "failure";
      correlationId: string;
      metadata?: Record<string, string | number | boolean | null>;
    }
  ): Promise<void> {
    await client.query(
      `INSERT INTO security_events
         (id, organization_id, user_id, event_type, outcome, correlation_id, metadata)
       VALUES ($1, $2, $3, $4, $5, $6, $7::jsonb)`,
      [
        randomUUID(),
        input.organizationId,
        input.userId,
        input.eventType,
        input.outcome,
        input.correlationId,
        JSON.stringify(input.metadata ?? {})
      ]
    );
  }

  async health(): Promise<boolean> {
    try {
      await this.pool.query("SELECT 1");
      return true;
    } catch {
      return false;
    }
  }

  async getMembership(userId: string, organizationId: string): Promise<OrganizationMembershipSummary> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT m.id, m.organization_id, m.user_id, u.display_name, u.email,
                m.role_key, m.status, m.permissions, m.created_at
         FROM organization_memberships m
         JOIN users u ON u.id = m.user_id
         WHERE m.user_id = $1 AND m.status = 'active' AND u.status = 'active'`,
        [userId]
      );
      return mapMembership(requireFirst(result.rows as MembershipRow[], "Active membership not found"));
    });
  }

  async listMemberships(organizationId: string): Promise<OrganizationMembershipSummary[]> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT m.id, m.organization_id, m.user_id, u.display_name, u.email,
                m.role_key, m.status, m.permissions, m.created_at
         FROM organization_memberships m
         JOIN users u ON u.id = m.user_id
         ORDER BY u.display_name, m.id`
      );
      return (result.rows as MembershipRow[]).map(mapMembership);
    });
  }

  async createSession(input: CreateSessionInput): Promise<SessionSummary> {
    return this.withTenant(input.organizationId, async (client) => {
      const membership = await client.query(
        "SELECT id FROM organization_memberships WHERE user_id = $1 AND status = 'active'",
        [input.userId]
      );
      if (membership.rowCount !== 1) throw new Error("Active membership not found");
      const result = await client.query(
        `INSERT INTO identity_sessions
           (id, organization_id, user_id, authentication_methods, expires_at)
         VALUES ($1, $2, $3, $4::jsonb, $5)
         RETURNING id, organization_id, user_id, authentication_methods,
                   issued_at, expires_at, revoked_at, revoked_by`,
        [randomUUID(), input.organizationId, input.userId, JSON.stringify(input.authenticationMethods), input.expiresAt]
      );
      const session = mapSession(requireFirst(result.rows as SessionRow[], "Session insert returned no row"));
      await this.appendSecurityEvent(client, {
        organizationId: input.organizationId,
        userId: input.userId,
        eventType: "SESSION_ISSUED",
        outcome: "success",
        correlationId: input.correlationId,
        metadata: { sessionId: session.id }
      });
      return session;
    });
  }

  async validateSession(sessionId: string, userId: string, organizationId: string): Promise<boolean> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT 1
         FROM identity_sessions
         WHERE id = $1 AND user_id = $2 AND revoked_at IS NULL AND expires_at > now()`,
        [sessionId, userId]
      );
      return result.rowCount === 1;
    });
  }

  async listSessions(organizationId: string, userId: string): Promise<SessionSummary[]> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT id, organization_id, user_id, authentication_methods,
                issued_at, expires_at, revoked_at, revoked_by
         FROM identity_sessions
         WHERE user_id = $1
         ORDER BY issued_at DESC, id DESC`,
        [userId]
      );
      return (result.rows as SessionRow[]).map(mapSession);
    });
  }

  async revokeSession(
    organizationId: string,
    sessionId: string,
    userId: string,
    revokedBy: string,
    correlationId: string
  ): Promise<SessionSummary> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `UPDATE identity_sessions
         SET revoked_at = COALESCE(revoked_at, now()), revoked_by = COALESCE(revoked_by, $3)
         WHERE id = $1 AND user_id = $2
         RETURNING id, organization_id, user_id, authentication_methods,
                   issued_at, expires_at, revoked_at, revoked_by`,
        [sessionId, userId, revokedBy]
      );
      const session = mapSession(requireFirst(result.rows as SessionRow[], "Session not found"));
      await this.appendSecurityEvent(client, {
        organizationId,
        userId,
        eventType: "SESSION_REVOKED",
        outcome: "success",
        correlationId,
        metadata: { sessionId }
      });
      return session;
    });
  }

  async listMfaMethods(organizationId: string, userId: string): Promise<MfaMethodSummary[]> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT id, organization_id, user_id, method, label, status, created_at, verified_at
         FROM mfa_methods
         WHERE user_id = $1
         ORDER BY created_at DESC, id DESC`,
        [userId]
      );
      return (result.rows as MfaRow[]).map(mapMfa);
    });
  }

  async enrollMfaMethod(
    organizationId: string,
    userId: string,
    method: MfaMethodSummary["method"],
    label: string | undefined,
    correlationId: string
  ): Promise<MfaMethodSummary> {
    return this.withTenant(organizationId, async (client) => {
      const membership = await client.query(
        "SELECT id FROM organization_memberships WHERE user_id = $1 AND status = 'active'",
        [userId]
      );
      if (membership.rowCount !== 1) throw new Error("Active membership not found");
      const result = await client.query(
        `INSERT INTO mfa_methods (id, organization_id, user_id, method, label, status)
         VALUES ($1, $2, $3, $4, $5, 'pending')
         RETURNING id, organization_id, user_id, method, label, status, created_at, verified_at`,
        [randomUUID(), organizationId, userId, method, label?.trim() || null]
      );
      const record = mapMfa(requireFirst(result.rows as MfaRow[], "MFA enrollment returned no row"));
      await this.appendSecurityEvent(client, {
        organizationId,
        userId,
        eventType: "MFA_ENROLLMENT_STARTED",
        outcome: "success",
        correlationId,
        metadata: { method, mfaMethodId: record.id }
      });
      return record;
    });
  }

  async close(): Promise<void> {
    await this.pool.end();
  }
}

export function createIdentityStore(config: Pick<ApiConfig, "storageMode" | "databaseUrl">): IdentityStore {
  if (config.storageMode === "postgres") {
    if (!config.databaseUrl) throw new Error("PostgreSQL identity storage requires a database URL");
    return new PostgresIdentityStore(config.databaseUrl);
  }
  return new InMemoryIdentityStore();
}

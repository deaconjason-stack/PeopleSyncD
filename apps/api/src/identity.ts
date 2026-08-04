import { randomUUID } from "node:crypto";
import { Pool, type PoolClient } from "pg";
import {
  createRecoveryCodes,
  createTotpSecret,
  decryptCredential,
  encryptCredential,
  hashRecoveryCode,
  totpProvisioningUri,
  verifyTotpCode
} from "@peoplesyncd/auth";
import { GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import type {
  MfaMethodSummary,
  OrganizationMembershipSummary,
  Permission,
  SessionSummary,
  TotpEnrollmentResult,
  TotpVerificationResult
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
  updateMembership(
    organizationId: string,
    userId: string,
    status: OrganizationMembershipSummary["status"],
    permissions: Permission[],
    actorId: string,
    correlationId: string
  ): Promise<OrganizationMembershipSummary>;
  createSession(input: CreateSessionInput): Promise<SessionSummary>;
  validateSession(sessionId: string, userId: string, organizationId: string): Promise<boolean>;
  listSessions(organizationId: string, userId: string): Promise<SessionSummary[]>;
  revokeSession(organizationId: string, sessionId: string, userId: string, revokedBy: string, correlationId: string): Promise<SessionSummary>;
  rotateSession(
    organizationId: string,
    sessionId: string,
    userId: string,
    authenticationMethods: string[],
    expiresAt: string,
    correlationId: string
  ): Promise<SessionSummary>;
  listMfaMethods(organizationId: string, userId: string): Promise<MfaMethodSummary[]>;
  enrollMfaMethod(
    organizationId: string,
    userId: string,
    method: MfaMethodSummary["method"],
    label: string | undefined,
    correlationId: string
  ): Promise<MfaMethodSummary>;
  enrollTotpMethod(
    organizationId: string,
    userId: string,
    label: string | undefined,
    accountName: string,
    issuer: string,
    correlationId: string
  ): Promise<TotpEnrollmentResult>;
  verifyTotpMethod(
    organizationId: string,
    userId: string,
    methodId: string,
    code: string,
    currentSessionId: string,
    expiresAt: string,
    correlationId: string
  ): Promise<TotpVerificationResult>;
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
  "identity.session.rotate",
  "identity.mfa.read",
  "identity.mfa.enroll",
  "identity.mfa.verify",
  "organization.membership.read",
  "organization.membership.manage"
];

function uniqueMethods(methods: string[]): string[] {
  return [...new Set(methods.map((method) => method.trim()).filter(Boolean))];
}

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
  private readonly mfaSecrets = new Map<string, string>();
  private readonly recoveryCodeHashes = new Map<string, string[]>();

  constructor(private readonly mfaEncryptionKey = "genesis-development-secret-change-me") {}

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

  async updateMembership(
    organizationId: string,
    userId: string,
    status: OrganizationMembershipSummary["status"],
    permissions: Permission[],
    actorId: string,
    _correlationId: string
  ): Promise<OrganizationMembershipSummary> {
    const membership = this.memberships.find(
      (candidate) => candidate.organizationId === organizationId && candidate.userId === userId
    );
    if (!membership) throw new Error("Membership not found");
    membership.status = status;
    membership.permissions = [...new Set(permissions)];
    if (status !== "active") {
      for (const session of this.sessions) {
        if (session.organizationId === organizationId && session.userId === userId && !session.revokedAt) {
          session.revokedAt = new Date().toISOString();
          session.revokedBy = actorId;
        }
      }
    }
    return { ...membership, permissions: [...membership.permissions] };
  }

  async createSession(input: CreateSessionInput): Promise<SessionSummary> {
    await this.getMembership(input.userId, input.organizationId);
    const id = randomUUID();
    const session: SessionSummary = {
      id,
      organizationId: input.organizationId,
      userId: input.userId,
      sessionFamilyId: id,
      authenticationMethods: uniqueMethods(input.authenticationMethods),
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

  async rotateSession(
    organizationId: string,
    sessionId: string,
    userId: string,
    authenticationMethods: string[],
    expiresAt: string,
    _correlationId: string
  ): Promise<SessionSummary> {
    const current = this.sessions.find(
      (candidate) =>
        candidate.id === sessionId &&
        candidate.organizationId === organizationId &&
        candidate.userId === userId &&
        !candidate.revokedAt
    );
    if (!current) throw new Error("Active session not found");
    const replacementId = randomUUID();
    current.revokedAt = new Date().toISOString();
    current.revokedBy = userId;
    current.replacedBy = replacementId;
    const replacement: SessionSummary = {
      id: replacementId,
      organizationId,
      userId,
      sessionFamilyId: current.sessionFamilyId,
      authenticationMethods: uniqueMethods(authenticationMethods),
      issuedAt: new Date().toISOString(),
      expiresAt,
      rotatedFrom: current.id
    };
    this.sessions.push(replacement);
    return { ...replacement, authenticationMethods: [...replacement.authenticationMethods] };
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

  async enrollTotpMethod(
    organizationId: string,
    userId: string,
    label: string | undefined,
    accountName: string,
    issuer: string,
    correlationId: string
  ): Promise<TotpEnrollmentResult> {
    const method = await this.enrollMfaMethod(organizationId, userId, "totp", label, correlationId);
    const secret = createTotpSecret();
    this.mfaSecrets.set(method.id, encryptCredential(secret, this.mfaEncryptionKey));
    return { method, provisioningUri: totpProvisioningUri({ secret, accountName, issuer }) };
  }

  async verifyTotpMethod(
    organizationId: string,
    userId: string,
    methodId: string,
    code: string,
    currentSessionId: string,
    expiresAt: string,
    correlationId: string
  ): Promise<TotpVerificationResult> {
    const method = this.mfaMethods.find(
      (candidate) => candidate.id === methodId && candidate.organizationId === organizationId && candidate.userId === userId
    );
    const encryptedSecret = this.mfaSecrets.get(methodId);
    if (!method || method.method !== "totp" || method.status === "revoked" || !encryptedSecret) {
      throw new Error("TOTP enrollment not found");
    }
    const secret = decryptCredential(encryptedSecret, this.mfaEncryptionKey);
    if (!verifyTotpCode(secret, code)) throw new Error("Invalid MFA code");
    method.status = "active";
    method.verifiedAt = new Date().toISOString();
    const current = this.sessions.find((session) => session.id === currentSessionId);
    if (!current) throw new Error("Active session not found");
    const session = await this.rotateSession(
      organizationId,
      currentSessionId,
      userId,
      uniqueMethods([...current.authenticationMethods, "totp"]),
      expiresAt,
      correlationId
    );
    const recoveryCodes = createRecoveryCodes();
    this.recoveryCodeHashes.set(method.id, recoveryCodes.map((recoveryCode) => hashRecoveryCode(recoveryCode, this.mfaEncryptionKey)));
    return { method: { ...method }, session, recoveryCodes };
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
  session_family_id: string;
  authentication_methods: string[];
  issued_at: Date | string;
  expires_at: Date | string;
  rotated_from: string | null;
  replaced_by: string | null;
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
  secret_ciphertext?: string | null;
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
    sessionFamilyId: row.session_family_id,
    authenticationMethods: row.authentication_methods,
    issuedAt: iso(row.issued_at),
    expiresAt: iso(row.expires_at),
    rotatedFrom: row.rotated_from ?? undefined,
    replacedBy: row.replaced_by ?? undefined,
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

  constructor(databaseUrl: string, private readonly mfaEncryptionKey = "genesis-development-secret-change-me") {
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

  async updateMembership(
    organizationId: string,
    userId: string,
    status: OrganizationMembershipSummary["status"],
    permissions: Permission[],
    actorId: string,
    correlationId: string
  ): Promise<OrganizationMembershipSummary> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `UPDATE organization_memberships
         SET status = $2, permissions = $3::jsonb, updated_at = now()
         WHERE user_id = $1
         RETURNING id`,
        [userId, status, JSON.stringify([...new Set(permissions)])]
      );
      const membership = requireFirst(result.rows as Array<{ id: string }>, "Membership not found");
      if (status !== "active") {
        await client.query(
          `UPDATE identity_sessions
           SET revoked_at = COALESCE(revoked_at, now()), revoked_by = COALESCE(revoked_by, $2)
           WHERE user_id = $1 AND revoked_at IS NULL`,
          [userId, actorId]
        );
      }
      await this.appendSecurityEvent(client, {
        organizationId,
        userId,
        eventType: "MEMBERSHIP_UPDATED",
        outcome: "success",
        correlationId,
        metadata: { actorId, status, permissionCount: permissions.length }
      });
      const joined = await client.query(
        `SELECT m.id, m.organization_id, m.user_id, u.display_name, u.email,
                m.role_key, m.status, m.permissions, m.created_at
         FROM organization_memberships m
         JOIN users u ON u.id = m.user_id
         WHERE m.id = $1`,
        [membership.id]
      );
      return mapMembership(requireFirst(joined.rows as MembershipRow[], "Membership not found"));
    });
  }

  async createSession(input: CreateSessionInput): Promise<SessionSummary> {
    return this.withTenant(input.organizationId, async (client) => {
      const membership = await client.query(
        "SELECT id FROM organization_memberships WHERE user_id = $1 AND status = 'active'",
        [input.userId]
      );
      if (membership.rowCount !== 1) throw new Error("Active membership not found");
      const id = randomUUID();
      const result = await client.query(
        `INSERT INTO identity_sessions
           (id, organization_id, user_id, session_family_id, authentication_methods, expires_at)
         VALUES ($1, $2, $3, $1, $4::jsonb, $5)
         RETURNING id, organization_id, user_id, session_family_id, authentication_methods,
                   issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by`,
        [id, input.organizationId, input.userId, JSON.stringify(uniqueMethods(input.authenticationMethods)), input.expiresAt]
      );
      const session = mapSession(requireFirst(result.rows as SessionRow[], "Session insert returned no row"));
      await this.appendSecurityEvent(client, {
        organizationId: input.organizationId,
        userId: input.userId,
        eventType: "SESSION_ISSUED",
        outcome: "success",
        correlationId: input.correlationId,
        metadata: { sessionId: session.id, sessionFamilyId: session.sessionFamilyId }
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
        `SELECT id, organization_id, user_id, session_family_id, authentication_methods,
                issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by
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
         RETURNING id, organization_id, user_id, session_family_id, authentication_methods,
                   issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by`,
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

  async rotateSession(
    organizationId: string,
    sessionId: string,
    userId: string,
    authenticationMethods: string[],
    expiresAt: string,
    correlationId: string
  ): Promise<SessionSummary> {
    return this.withTenant(organizationId, async (client) => {
      const currentResult = await client.query(
        `SELECT id, organization_id, user_id, session_family_id, authentication_methods,
                issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by
         FROM identity_sessions
         WHERE id = $1 AND user_id = $2 AND revoked_at IS NULL AND expires_at > now()
         FOR UPDATE`,
        [sessionId, userId]
      );
      const current = mapSession(requireFirst(currentResult.rows as SessionRow[], "Active session not found"));
      const replacementId = randomUUID();
      await client.query(
        `UPDATE identity_sessions
         SET revoked_at = now(), revoked_by = $2, replaced_by = $3
         WHERE id = $1`,
        [sessionId, userId, replacementId]
      );
      const replacementResult = await client.query(
        `INSERT INTO identity_sessions
           (id, organization_id, user_id, session_family_id, authentication_methods, expires_at, rotated_from)
         VALUES ($1, $2, $3, $4, $5::jsonb, $6, $7)
         RETURNING id, organization_id, user_id, session_family_id, authentication_methods,
                   issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by`,
        [
          replacementId,
          organizationId,
          userId,
          current.sessionFamilyId,
          JSON.stringify(uniqueMethods(authenticationMethods)),
          expiresAt,
          sessionId
        ]
      );
      const replacement = mapSession(requireFirst(replacementResult.rows as SessionRow[], "Session rotation returned no row"));
      await this.appendSecurityEvent(client, {
        organizationId,
        userId,
        eventType: "SESSION_ROTATED",
        outcome: "success",
        correlationId,
        metadata: { previousSessionId: sessionId, sessionId: replacement.id, sessionFamilyId: replacement.sessionFamilyId }
      });
      return replacement;
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

  async enrollTotpMethod(
    organizationId: string,
    userId: string,
    label: string | undefined,
    accountName: string,
    issuer: string,
    correlationId: string
  ): Promise<TotpEnrollmentResult> {
    const secret = createTotpSecret();
    const encryptedSecret = encryptCredential(secret, this.mfaEncryptionKey);
    const method = await this.withTenant(organizationId, async (client) => {
      const membership = await client.query(
        "SELECT id FROM organization_memberships WHERE user_id = $1 AND status = 'active'",
        [userId]
      );
      if (membership.rowCount !== 1) throw new Error("Active membership not found");
      const result = await client.query(
        `INSERT INTO mfa_methods
           (id, organization_id, user_id, method, label, status, secret_ciphertext)
         VALUES ($1, $2, $3, 'totp', $4, 'pending', $5)
         RETURNING id, organization_id, user_id, method, label, status, created_at, verified_at`,
        [randomUUID(), organizationId, userId, label?.trim() || null, encryptedSecret]
      );
      const record = mapMfa(requireFirst(result.rows as MfaRow[], "TOTP enrollment returned no row"));
      await this.appendSecurityEvent(client, {
        organizationId,
        userId,
        eventType: "MFA_TOTP_ENROLLMENT_STARTED",
        outcome: "success",
        correlationId,
        metadata: { mfaMethodId: record.id }
      });
      return record;
    });
    return { method, provisioningUri: totpProvisioningUri({ secret, accountName, issuer }) };
  }

  async verifyTotpMethod(
    organizationId: string,
    userId: string,
    methodId: string,
    code: string,
    currentSessionId: string,
    expiresAt: string,
    correlationId: string
  ): Promise<TotpVerificationResult> {
    const result = await this.withTenant(organizationId, async (client) => {
      const methodResult = await client.query(
        `SELECT id, organization_id, user_id, method, label, status, created_at, verified_at, secret_ciphertext
         FROM mfa_methods
         WHERE id = $1 AND user_id = $2 AND method = 'totp' AND status <> 'revoked'
         FOR UPDATE`,
        [methodId, userId]
      );
      const row = requireFirst(methodResult.rows as MfaRow[], "TOTP enrollment not found");
      if (!row.secret_ciphertext) throw new Error("TOTP credential is unavailable");
      const secret = decryptCredential(row.secret_ciphertext, this.mfaEncryptionKey);
      if (!verifyTotpCode(secret, code)) {
        await client.query(
          `UPDATE mfa_methods
           SET failed_attempts = failed_attempts + 1, last_failed_at = now()
           WHERE id = $1`,
          [methodId]
        );
        await this.appendSecurityEvent(client, {
          organizationId,
          userId,
          eventType: "MFA_TOTP_VERIFICATION_FAILED",
          outcome: "denied",
          correlationId,
          metadata: { mfaMethodId: methodId }
        });
        return { valid: false as const };
      }

      const activatedResult = await client.query(
        `UPDATE mfa_methods
         SET status = 'active', verified_at = COALESCE(verified_at, now()), failed_attempts = 0, last_failed_at = NULL
         WHERE id = $1
         RETURNING id, organization_id, user_id, method, label, status, created_at, verified_at`,
        [methodId]
      );
      const method = mapMfa(requireFirst(activatedResult.rows as MfaRow[], "TOTP activation returned no row"));

      const currentResult = await client.query(
        `SELECT id, organization_id, user_id, session_family_id, authentication_methods,
                issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by
         FROM identity_sessions
         WHERE id = $1 AND user_id = $2 AND revoked_at IS NULL AND expires_at > now()
         FOR UPDATE`,
        [currentSessionId, userId]
      );
      const current = mapSession(requireFirst(currentResult.rows as SessionRow[], "Active session not found"));
      const replacementId = randomUUID();
      await client.query(
        `UPDATE identity_sessions
         SET revoked_at = now(), revoked_by = $2, replaced_by = $3
         WHERE id = $1`,
        [currentSessionId, userId, replacementId]
      );
      const replacementResult = await client.query(
        `INSERT INTO identity_sessions
           (id, organization_id, user_id, session_family_id, authentication_methods, expires_at, rotated_from)
         VALUES ($1, $2, $3, $4, $5::jsonb, $6, $7)
         RETURNING id, organization_id, user_id, session_family_id, authentication_methods,
                   issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by`,
        [
          replacementId,
          organizationId,
          userId,
          current.sessionFamilyId,
          JSON.stringify(uniqueMethods([...current.authenticationMethods, "totp"])),
          expiresAt,
          currentSessionId
        ]
      );
      const session = mapSession(requireFirst(replacementResult.rows as SessionRow[], "MFA session rotation returned no row"));

      const recoveryCodes = createRecoveryCodes();
      for (const recoveryCode of recoveryCodes) {
        await client.query(
          `INSERT INTO mfa_recovery_codes
             (id, organization_id, user_id, mfa_method_id, code_hash)
           VALUES ($1, $2, $3, $4, $5)`,
          [randomUUID(), organizationId, userId, methodId, hashRecoveryCode(recoveryCode, this.mfaEncryptionKey)]
        );
      }
      await this.appendSecurityEvent(client, {
        organizationId,
        userId,
        eventType: "MFA_TOTP_VERIFIED",
        outcome: "success",
        correlationId,
        metadata: { mfaMethodId: methodId, sessionId: session.id, sessionFamilyId: session.sessionFamilyId }
      });
      return { valid: true as const, value: { method, session, recoveryCodes } };
    });
    if (!result.valid) throw new Error("Invalid MFA code");
    return result.value;
  }

  async close(): Promise<void> {
    await this.pool.end();
  }
}

export function createIdentityStore(config: Pick<ApiConfig, "storageMode" | "databaseUrl" | "mfaEncryptionKey">): IdentityStore {
  if (config.storageMode === "postgres") {
    if (!config.databaseUrl) throw new Error("PostgreSQL identity storage requires a database URL");
    return new PostgresIdentityStore(config.databaseUrl, config.mfaEncryptionKey);
  }
  return new InMemoryIdentityStore(config.mfaEncryptionKey);
}

import { randomUUID } from "node:crypto";
import { Pool, type PoolClient } from "pg";
import {
  createRecoveryCodes,
  createTotpSecret,
  decryptCredential,
  encryptCredential,
  hashRecoveryCode,
  matchTotpCounter,
  totpProvisioningUri
} from "@peoplesyncd/auth";
import type {
  MfaMethodSummary,
  OrganizationMembershipSummary,
  Permission,
  RecoveryCodeVerificationResult,
  SessionSummary,
  TotpEnrollmentResult,
  TotpVerificationResult
} from "@peoplesyncd/shared";
import type { CreateSessionInput, IdentityStore } from "./identity-contract";
import { createsFounderLockout, uniqueMethods } from "./identity-contract";

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
  last_totp_counter?: string | number | null;
}

function first<T>(rows: T[], message: string): T {
  const row = rows[0];
  if (!row) throw new Error(message);
  return row;
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

  constructor(
    databaseUrl: string,
    private readonly mfaEncryptionKey = "genesis-development-secret-change-me",
    private readonly assumeRuntimeRole = true
  ) {
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
      if (this.assumeRuntimeRole) await client.query("SET LOCAL ROLE peoplesyncd_runtime");
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
      return mapMembership(first(result.rows as MembershipRow[], "Active membership not found"));
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
      const normalizedPermissions = [...new Set(permissions)];
      const currentResult = await client.query(
        `SELECT m.id, m.organization_id, m.user_id, u.display_name, u.email,
                m.role_key, m.status, m.permissions, m.created_at
         FROM organization_memberships m
         JOIN users u ON u.id = m.user_id
         WHERE m.user_id = $1
         FOR UPDATE OF m`,
        [userId]
      );
      const current = mapMembership(first(currentResult.rows as MembershipRow[], "Membership not found"));
      if (createsFounderLockout(current.roleKey, status, normalizedPermissions)) {
        const alternate = await client.query(
          `SELECT 1
           FROM organization_memberships
           WHERE user_id <> $1
             AND role_key = 'founder'
             AND status = 'active'
             AND permissions @> '["organization.membership.manage"]'::jsonb
           LIMIT 1`,
          [userId]
        );
        if (alternate.rowCount !== 1) throw new Error("Last Founder invariant violation");
      }

      await client.query(
        `UPDATE organization_memberships
         SET status = $2, permissions = $3::jsonb, updated_at = now()
         WHERE user_id = $1`,
        [userId, status, JSON.stringify(normalizedPermissions)]
      );
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
        metadata: { actorId, status, permissionCount: normalizedPermissions.length }
      });
      const joined = await client.query(
        `SELECT m.id, m.organization_id, m.user_id, u.display_name, u.email,
                m.role_key, m.status, m.permissions, m.created_at
         FROM organization_memberships m
         JOIN users u ON u.id = m.user_id
         WHERE m.user_id = $1`,
        [userId]
      );
      return mapMembership(first(joined.rows as MembershipRow[], "Membership not found"));
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
      const session = mapSession(first(result.rows as SessionRow[], "Session insert returned no row"));
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
        `SELECT 1 FROM identity_sessions
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
         FROM identity_sessions WHERE user_id = $1 ORDER BY issued_at DESC, id DESC`,
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
      const session = mapSession(first(result.rows as SessionRow[], "Session not found"));
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

  private async rotateWithinTransaction(
    client: PoolClient,
    input: {
      organizationId: string;
      sessionId: string;
      userId: string;
      authenticationMethods: string[];
      expiresAt: string;
      correlationId: string;
      eventType: string;
    }
  ): Promise<SessionSummary> {
    const currentResult = await client.query(
      `SELECT id, organization_id, user_id, session_family_id, authentication_methods,
              issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by
       FROM identity_sessions
       WHERE id = $1 AND user_id = $2 AND revoked_at IS NULL AND expires_at > now()
       FOR UPDATE`,
      [input.sessionId, input.userId]
    );
    const current = mapSession(first(currentResult.rows as SessionRow[], "Active session not found"));
    const replacementId = randomUUID();
    const replacementResult = await client.query(
      `INSERT INTO identity_sessions
         (id, organization_id, user_id, session_family_id, authentication_methods, expires_at, rotated_from)
       VALUES ($1, $2, $3, $4, $5::jsonb, $6, $7)
       RETURNING id, organization_id, user_id, session_family_id, authentication_methods,
                 issued_at, expires_at, rotated_from, replaced_by, revoked_at, revoked_by`,
      [
        replacementId,
        input.organizationId,
        input.userId,
        current.sessionFamilyId,
        JSON.stringify(uniqueMethods(input.authenticationMethods)),
        input.expiresAt,
        input.sessionId
      ]
    );
    await client.query(
      `UPDATE identity_sessions
       SET revoked_at = now(), revoked_by = $2, replaced_by = $3
       WHERE id = $1`,
      [input.sessionId, input.userId, replacementId]
    );
    const replacement = mapSession(first(replacementResult.rows as SessionRow[], "Session rotation returned no row"));
    await this.appendSecurityEvent(client, {
      organizationId: input.organizationId,
      userId: input.userId,
      eventType: input.eventType,
      outcome: "success",
      correlationId: input.correlationId,
      metadata: {
        previousSessionId: input.sessionId,
        sessionId: replacement.id,
        sessionFamilyId: replacement.sessionFamilyId
      }
    });
    return replacement;
  }

  async rotateSession(
    organizationId: string,
    sessionId: string,
    userId: string,
    authenticationMethods: string[],
    expiresAt: string,
    correlationId: string
  ): Promise<SessionSummary> {
    return this.withTenant(organizationId, (client) =>
      this.rotateWithinTransaction(client, {
        organizationId,
        sessionId,
        userId,
        authenticationMethods,
        expiresAt,
        correlationId,
        eventType: "SESSION_ROTATED"
      })
    );
  }

  async listMfaMethods(organizationId: string, userId: string): Promise<MfaMethodSummary[]> {
    return this.withTenant(organizationId, async (client) => {
      const result = await client.query(
        `SELECT id, organization_id, user_id, method, label, status, created_at, verified_at
         FROM mfa_methods WHERE user_id = $1 ORDER BY created_at DESC, id DESC`,
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
      const record = mapMfa(first(result.rows as MfaRow[], "MFA enrollment returned no row"));
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
      const record = mapMfa(first(result.rows as MfaRow[], "TOTP enrollment returned no row"));
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
        `SELECT id, organization_id, user_id, method, label, status, created_at, verified_at,
                secret_ciphertext, last_totp_counter
         FROM mfa_methods
         WHERE id = $1 AND user_id = $2 AND method = 'totp' AND status <> 'revoked'
         FOR UPDATE`,
        [methodId, userId]
      );
      const row = first(methodResult.rows as MfaRow[], "TOTP enrollment not found");
      if (!row.secret_ciphertext) throw new Error("TOTP credential is unavailable");
      const counter = matchTotpCounter(decryptCredential(row.secret_ciphertext, this.mfaEncryptionKey), code);
      if (counter === undefined) {
        await client.query(
          `UPDATE mfa_methods SET failed_attempts = failed_attempts + 1, last_failed_at = now() WHERE id = $1`,
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
        return { valid: false as const, replay: false as const };
      }
      const previousCounter = row.last_totp_counter === null || row.last_totp_counter === undefined
        ? undefined
        : Number(row.last_totp_counter);
      if (previousCounter !== undefined && counter <= previousCounter) {
        await this.appendSecurityEvent(client, {
          organizationId,
          userId,
          eventType: "MFA_TOTP_REPLAY_DENIED",
          outcome: "denied",
          correlationId,
          metadata: { mfaMethodId: methodId, counter }
        });
        return { valid: false as const, replay: true as const };
      }

      const wasPending = row.status === "pending";
      const activated = await client.query(
        `UPDATE mfa_methods
         SET status = 'active', verified_at = COALESCE(verified_at, now()),
             failed_attempts = 0, last_failed_at = NULL, last_totp_counter = $2
         WHERE id = $1
         RETURNING id, organization_id, user_id, method, label, status, created_at, verified_at`,
        [methodId, counter]
      );
      const method = mapMfa(first(activated.rows as MfaRow[], "TOTP activation returned no row"));
      const current = await client.query(
        `SELECT authentication_methods FROM identity_sessions
         WHERE id = $1 AND user_id = $2 AND revoked_at IS NULL AND expires_at > now()`,
        [currentSessionId, userId]
      );
      const currentMethods = first(current.rows as Array<{ authentication_methods: string[] }>, "Active session not found")
        .authentication_methods;
      const session = await this.rotateWithinTransaction(client, {
        organizationId,
        sessionId: currentSessionId,
        userId,
        authenticationMethods: uniqueMethods([...currentMethods, "totp"]),
        expiresAt,
        correlationId,
        eventType: "MFA_TOTP_SESSION_ROTATED"
      });

      const recoveryCodes = wasPending ? createRecoveryCodes() : [];
      if (wasPending) {
        for (const recoveryCode of recoveryCodes) {
          await client.query(
            `INSERT INTO mfa_recovery_codes
               (id, organization_id, user_id, mfa_method_id, code_hash)
             VALUES ($1, $2, $3, $4, $5)`,
            [randomUUID(), organizationId, userId, methodId, hashRecoveryCode(recoveryCode, this.mfaEncryptionKey)]
          );
        }
      }
      await this.appendSecurityEvent(client, {
        organizationId,
        userId,
        eventType: "MFA_TOTP_VERIFIED",
        outcome: "success",
        correlationId,
        metadata: { mfaMethodId: methodId, sessionId: session.id, counter }
      });
      return { valid: true as const, value: { method, session, recoveryCodes } };
    });
    if (!result.valid) throw new Error(result.replay ? "MFA code replay detected" : "Invalid MFA code");
    return result.value;
  }

  async consumeRecoveryCode(
    organizationId: string,
    userId: string,
    code: string,
    currentSessionId: string,
    expiresAt: string,
    correlationId: string
  ): Promise<RecoveryCodeVerificationResult> {
    return this.withTenant(organizationId, async (client) => {
      const codeHash = hashRecoveryCode(code, this.mfaEncryptionKey);
      const recovery = await client.query(
        `SELECT id FROM mfa_recovery_codes
         WHERE user_id = $1 AND code_hash = $2 AND used_at IS NULL
         FOR UPDATE`,
        [userId, codeHash]
      );
      const record = first(recovery.rows as Array<{ id: string }>, "Invalid or used recovery code");
      await client.query("UPDATE mfa_recovery_codes SET used_at = now() WHERE id = $1", [record.id]);
      const current = await client.query(
        `SELECT authentication_methods FROM identity_sessions
         WHERE id = $1 AND user_id = $2 AND revoked_at IS NULL AND expires_at > now()`,
        [currentSessionId, userId]
      );
      const currentMethods = first(current.rows as Array<{ authentication_methods: string[] }>, "Active session not found")
        .authentication_methods;
      const session = await this.rotateWithinTransaction(client, {
        organizationId,
        sessionId: currentSessionId,
        userId,
        authenticationMethods: uniqueMethods([...currentMethods, "recovery_code"]),
        expiresAt,
        correlationId,
        eventType: "MFA_RECOVERY_SESSION_ROTATED"
      });
      const remaining = await client.query(
        "SELECT count(*)::integer AS count FROM mfa_recovery_codes WHERE user_id = $1 AND used_at IS NULL",
        [userId]
      );
      const remainingCodes = Number(first(remaining.rows as Array<{ count: number }>, "Recovery count unavailable").count);
      await this.appendSecurityEvent(client, {
        organizationId,
        userId,
        eventType: "MFA_RECOVERY_CODE_USED",
        outcome: "success",
        correlationId,
        metadata: { sessionId: session.id, remainingCodes }
      });
      return { session, remainingCodes };
    });
  }

  async close(): Promise<void> {
    await this.pool.end();
  }
}

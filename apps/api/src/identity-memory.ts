import { randomUUID } from "node:crypto";
import {
  createRecoveryCodes,
  createTotpSecret,
  decryptCredential,
  encryptCredential,
  hashRecoveryCode,
  matchTotpCounter,
  totpProvisioningUri
} from "@peoplesyncd/auth";
import { GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
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
import { createsFounderLockout, FOUNDER_PERMISSIONS, uniqueMethods } from "./identity-contract";

interface RecoveryRecord {
  hash: string;
  usedAt?: string;
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
      permissions: [...FOUNDER_PERMISSIONS],
      createdAt: new Date("2026-08-03T12:00:00Z").toISOString()
    }
  ];
  private readonly sessions: SessionSummary[] = [];
  private readonly mfaMethods: MfaMethodSummary[] = [];
  private readonly mfaSecrets = new Map<string, string>();
  private readonly lastTotpCounters = new Map<string, number>();
  private readonly recoveryCodes = new Map<string, RecoveryRecord[]>();

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
    const normalizedPermissions = [...new Set(permissions)];
    if (createsFounderLockout(membership.roleKey, status, normalizedPermissions)) {
      const alternateFounder = this.memberships.some(
        (candidate) =>
          candidate.organizationId === organizationId &&
          candidate.userId !== userId &&
          candidate.roleKey === "founder" &&
          candidate.status === "active" &&
          candidate.permissions.includes("organization.membership.manage")
      );
      if (!alternateFounder) throw new Error("Last Founder invariant violation");
    }
    membership.status = status;
    membership.permissions = normalizedPermissions;
    if (status !== "active") this.revokeUserSessions(organizationId, userId, actorId);
    return { ...membership, permissions: [...membership.permissions] };
  }

  private revokeUserSessions(organizationId: string, userId: string, actorId: string): void {
    for (const session of this.sessions) {
      if (session.organizationId === organizationId && session.userId === userId && !session.revokedAt) {
        session.revokedAt = new Date().toISOString();
        session.revokedBy = actorId;
      }
    }
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
    return this.copySession(session);
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
      .map((session) => this.copySession(session));
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
    return this.copySession(session);
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
        !candidate.revokedAt &&
        Date.parse(candidate.expiresAt) > Date.now()
    );
    if (!current) throw new Error("Active session not found");
    const replacementId = randomUUID();
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
    current.revokedAt = new Date().toISOString();
    current.revokedBy = userId;
    current.replacedBy = replacementId;
    return this.copySession(replacement);
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
    const counter = matchTotpCounter(decryptCredential(encryptedSecret, this.mfaEncryptionKey), code);
    if (counter === undefined) throw new Error("Invalid MFA code");
    const previousCounter = this.lastTotpCounters.get(methodId);
    if (previousCounter !== undefined && counter <= previousCounter) throw new Error("MFA code replay detected");
    this.lastTotpCounters.set(methodId, counter);

    const wasPending = method.status === "pending";
    method.status = "active";
    method.verifiedAt ??= new Date().toISOString();
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
    const recoveryCodes = wasPending ? createRecoveryCodes() : [];
    if (wasPending) {
      this.recoveryCodes.set(
        method.id,
        recoveryCodes.map((recoveryCode) => ({ hash: hashRecoveryCode(recoveryCode, this.mfaEncryptionKey) }))
      );
    }
    return { method: { ...method }, session, recoveryCodes };
  }

  async consumeRecoveryCode(
    organizationId: string,
    userId: string,
    code: string,
    currentSessionId: string,
    expiresAt: string,
    correlationId: string
  ): Promise<RecoveryCodeVerificationResult> {
    const suppliedHash = hashRecoveryCode(code, this.mfaEncryptionKey);
    let matched: RecoveryRecord | undefined;
    for (const records of this.recoveryCodes.values()) {
      matched = records.find((record) => record.hash === suppliedHash && !record.usedAt);
      if (matched) break;
    }
    if (!matched) throw new Error("Invalid or used recovery code");
    matched.usedAt = new Date().toISOString();
    const current = this.sessions.find((session) => session.id === currentSessionId);
    if (!current) throw new Error("Active session not found");
    const session = await this.rotateSession(
      organizationId,
      currentSessionId,
      userId,
      uniqueMethods([...current.authenticationMethods, "recovery_code"]),
      expiresAt,
      correlationId
    );
    const remainingCodes = [...this.recoveryCodes.values()].flat().filter((record) => !record.usedAt).length;
    return { session, remainingCodes };
  }

  private copySession(session: SessionSummary): SessionSummary {
    return { ...session, authenticationMethods: [...session.authenticationMethods] };
  }

  async close(): Promise<void> {
    return undefined;
  }
}

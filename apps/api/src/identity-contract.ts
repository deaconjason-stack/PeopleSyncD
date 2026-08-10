import type {
  MfaMethodSummary,
  OrganizationMembershipSummary,
  Permission,
  RecoveryCodeVerificationResult,
  SessionSummary,
  TotpEnrollmentResult,
  TotpVerificationResult
} from "@peoplesyncd/shared";

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
  revokeSession(
    organizationId: string,
    sessionId: string,
    userId: string,
    revokedBy: string,
    correlationId: string
  ): Promise<SessionSummary>;
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
  consumeRecoveryCode(
    organizationId: string,
    userId: string,
    code: string,
    currentSessionId: string,
    expiresAt: string,
    correlationId: string
  ): Promise<RecoveryCodeVerificationResult>;
  close(): Promise<void>;
}

export const FOUNDER_PERMISSIONS: Permission[] = [
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
  "identity.mfa.recovery.consume",
  "organization.membership.read",
  "organization.membership.manage"
];

export function uniqueMethods(methods: string[]): string[] {
  return [...new Set(methods.map((method) => method.trim()).filter(Boolean))];
}

export function createsFounderLockout(
  roleKey: string,
  status: OrganizationMembershipSummary["status"],
  permissions: Permission[]
): boolean {
  return roleKey === "founder" && (status !== "active" || !permissions.includes("organization.membership.manage"));
}

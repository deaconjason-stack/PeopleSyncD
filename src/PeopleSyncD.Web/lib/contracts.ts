export type TenantRole = 'Owner' | 'Administrator' | 'Manager' | 'Member' | 'Auditor';
export type MembershipStatus = 'Active' | 'Suspended' | 'Revoked';
export type InvitationStatus = 'Pending' | 'Accepted' | 'Revoked' | 'Expired';
export type AssuranceLevel = 'pwd' | 'mfa';
export type MfaMethod = 'totp' | 'recovery_code';

export interface IdentityUser {
  id: string;
  displayName: string;
  email: string;
  emailConfirmed: boolean;
  isActive: boolean;
  mfaEnabled: boolean;
}

export interface OrganizationAccess {
  membershipId: string;
  organizationId: string;
  organizationName: string;
  organizationSlug: string;
  role: TenantRole;
  status: MembershipStatus;
}

export interface TenantContext {
  membershipId: string;
  organizationId: string;
  organizationName: string;
  organizationSlug: string;
  role: TenantRole;
  permissions: string[];
}

export interface AccessToken {
  accessToken: string;
  tokenType: 'Bearer';
  expiresAt: string;
  user: IdentityUser;
  tenant: TenantContext | null;
  refreshToken: string | null;
  refreshTokenExpiresAt: string | null;
  assuranceLevel: AssuranceLevel;
  sessionFamilyId: string | null;
}

export interface CurrentSession {
  user: IdentityUser;
  tenant: TenantContext | null;
}

export interface MembershipAdmin {
  membershipId: string;
  userId: string;
  organizationId: string;
  displayName: string;
  email: string;
  role: TenantRole;
  status: MembershipStatus;
  emailConfirmed: boolean;
  mfaEnabled: boolean;
}

export interface Invitation {
  id: string;
  organizationId: string;
  email: string;
  displayName: string;
  role: TenantRole;
  status: InvitationStatus;
  createdAt: string;
  expiresAt: string;
}

export interface MfaChallenge {
  challengeToken: string;
  expiresAt: string;
  methods: MfaMethod[];
  purpose: 'login' | 'step_up';
}

export interface MfaTotpEnrollment {
  manualEntryKey: string;
  otpauthUri: string;
}

export interface RecoveryCodeBatch {
  recoveryCodes: string[];
  generatedAt: string;
}

export interface AccountSecurity {
  userId: string;
  emailConfirmed: boolean;
  mfaEnabled: boolean;
  passwordOnlyLoginAllowed: boolean;
  recoveryCodesRemaining: number;
}

export interface SessionSummary {
  familyId: string;
  createdAt: string;
  expiresAt: string;
  lastSeenAt: string;
  assuranceLevel: AssuranceLevel;
  deviceLabel: string | null;
  isCurrent: boolean;
}

export interface SecurityEvent {
  eventType: string;
  occurredAt: string;
  targetType: string;
  targetId: string;
}

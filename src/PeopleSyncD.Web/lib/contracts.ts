export type TenantRole = 'Owner' | 'Administrator' | 'Manager' | 'Member' | 'Auditor';
export type MembershipStatus = 'Active' | 'Suspended' | 'Revoked';
export type InvitationStatus = 'Pending' | 'Accepted' | 'Revoked' | 'Expired';

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

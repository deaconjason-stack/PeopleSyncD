export type TenantRole = 'Owner' | 'Administrator' | 'Manager' | 'Member' | 'Auditor';
export type MembershipStatus = 'Active' | 'Suspended' | 'Revoked';

export interface IdentityUser {
  id: string;
  displayName: string;
  email: string;
  emailConfirmed: boolean;
  isActive: boolean;
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
}

export interface CurrentSession {
  user: IdentityUser;
  tenant: TenantContext | null;
}

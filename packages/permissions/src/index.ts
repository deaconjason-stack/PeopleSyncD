import type { Permission, SessionClaims } from "@peoplesyncd/shared";

export class AuthorizationError extends Error {
  readonly statusCode = 403;
}

export function requireOrganization(claims: SessionClaims, organizationId: string | undefined): string {
  if (!organizationId) throw new AuthorizationError("Organization context is required");
  if (!claims.organizationIds.includes(organizationId)) {
    throw new AuthorizationError("Organization context is not authorized");
  }
  return organizationId;
}

export function requirePermission(claims: SessionClaims, permission: Permission): void {
  if (!claims.permissions.includes(permission)) {
    throw new AuthorizationError(`Permission denied: ${permission}`);
  }
}

export function authorize(claims: SessionClaims, organizationId: string | undefined, permission: Permission): string {
  const authorizedOrganization = requireOrganization(claims, organizationId);
  requirePermission(claims, permission);
  return authorizedOrganization;
}

import { randomUUID } from "node:crypto";
import type { FastifyRequest } from "fastify";
import { bearerToken, verifySessionToken } from "@peoplesyncd/auth";
import { authorize } from "@peoplesyncd/permissions";
import type { Permission, SessionClaims } from "@peoplesyncd/shared";
import type { ApiConfig } from "./config";
import type { IdentityStore } from "./identity";

export interface RequestContext {
  claims: SessionClaims;
  organizationId: string;
  correlationId: string;
}

function header(request: FastifyRequest, name: string): string | undefined {
  const value = request.headers[name];
  return Array.isArray(value) ? value[0] : value;
}

export async function requestContext(
  request: FastifyRequest,
  config: ApiConfig,
  identity: IdentityStore,
  permission: Permission
): Promise<RequestContext> {
  const token = bearerToken(request.headers.authorization);
  const claims = verifySessionToken(token, config.sessionSecret);
  const organizationId = authorize(claims, header(request, "x-organization-id"), permission);
  const active = await identity.validateSession(claims.sessionId, claims.subject, organizationId);
  if (!active) throw new Error("Session revoked or inactive");
  const correlationId = header(request, "x-correlation-id") ?? randomUUID();
  return { claims, organizationId, correlationId };
}

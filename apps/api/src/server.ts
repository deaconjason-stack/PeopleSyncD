import { randomUUID } from "node:crypto";
import Fastify from "fastify";
import cors from "@fastify/cors";
import { createSessionToken } from "@peoplesyncd/auth";
import { AuthorizationError } from "@peoplesyncd/permissions";
import { GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import type {
  MfaMethodSummary,
  OrganizationMembershipSummary,
  Permission,
  SessionClaims,
  SessionSummary,
  WorkerSummary
} from "@peoplesyncd/shared";
import type { ApiConfig } from "./config";
import { readConfig } from "./config";
import type { IdentityStore } from "./identity";
import { createIdentityStore } from "./identity";
import { requestContext } from "./security";
import type { PlatformStore } from "./store";
import { createPlatformStore } from "./store";

const KNOWN_PERMISSIONS = new Set<Permission>([
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
]);

function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : "Unknown error";
}

function sessionTokenResponse(
  membership: OrganizationMembershipSummary,
  session: SessionSummary,
  config: ApiConfig,
  mode?: string
) {
  const claims: SessionClaims = {
    sessionId: session.id,
    subject: membership.userId,
    displayName: membership.displayName,
    organizationIds: [membership.organizationId],
    permissions: membership.permissions,
    authenticationMethods: session.authenticationMethods,
    issuedAt: Math.floor(Date.parse(session.issuedAt) / 1000),
    expiresAt: Math.floor(Date.parse(session.expiresAt) / 1000)
  };
  return {
    token: createSessionToken(claims, config.sessionSecret),
    sessionId: session.id,
    sessionFamilyId: session.sessionFamilyId,
    organizationId: membership.organizationId,
    expiresAt: claims.expiresAt,
    authenticationMethods: claims.authenticationMethods,
    ...(mode ? { mode } : {})
  };
}

function parsePermissions(input: unknown): Permission[] {
  if (!Array.isArray(input)) throw new Error("permissions are required");
  const permissions = input.filter(
    (value): value is Permission => typeof value === "string" && KNOWN_PERMISSIONS.has(value as Permission)
  );
  if (permissions.length !== input.length) throw new Error("permissions contain an invalid value");
  return [...new Set(permissions)];
}

export function buildServer(
  config: ApiConfig = readConfig(),
  store: PlatformStore = createPlatformStore(config),
  identity: IdentityStore = createIdentityStore(config)
) {
  const app = Fastify({ logger: false, trustProxy: false, bodyLimit: 1024 * 1024 });
  void app.register(cors, { origin: config.corsOrigin, credentials: false });
  app.addHook("onClose", async () => {
    await Promise.all([store.close(), identity.close()]);
  });

  app.setErrorHandler((error, _request, reply) => {
    const message = errorMessage(error);
    if (error instanceof AuthorizationError || /session|bearer|signature|expired|revoked|inactive|MFA code/i.test(message)) {
      return reply.status(error instanceof AuthorizationError ? 403 : 401).send({ error: message });
    }
    if (/not found|invalid|required|unavailable/i.test(message)) {
      return reply.status(400).send({ error: message });
    }
    return reply.status(500).send({ error: "Internal server error" });
  });

  app.get("/health/live", async () => ({ status: "live", release: "0.4.0-identity-hardening" }));
  app.get("/health/ready", async (_request, reply) => {
    const [platformReady, identityReady] = await Promise.all([store.health(), identity.health()]);
    const ready = platformReady && identityReady;
    return reply.status(ready ? 200 : 503).send({
      status: ready ? "ready" : "not-ready",
      storage: store.kind,
      identityStorage: identity.kind,
      certified: false
    });
  });

  app.post("/v1/auth/dev-session", async (_request, reply) => {
    if (!config.devAuthEnabled) return reply.status(404).send({ error: "Not found" });
    const correlationId = randomUUID();
    const membership = await identity.getMembership("founder-jason", GENESIS_ORGANIZATION_ID);
    const session = await identity.createSession({
      organizationId: GENESIS_ORGANIZATION_ID,
      userId: membership.userId,
      authenticationMethods: ["development"],
      expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
      correlationId
    });
    return sessionTokenResponse(membership, session, config, "development-only");
  });

  app.get("/v1/auth/sessions", async (request) => {
    const context = await requestContext(request, config, identity, "identity.session.read");
    return identity.listSessions(context.organizationId, context.claims.subject);
  });

  app.post("/v1/auth/logout", async (request, reply) => {
    const context = await requestContext(request, config, identity, "identity.session.revoke");
    await identity.revokeSession(
      context.organizationId,
      context.claims.sessionId,
      context.claims.subject,
      context.claims.subject,
      context.correlationId
    );
    return reply.status(204).send();
  });

  app.post("/v1/auth/session/rotate", async (request) => {
    const context = await requestContext(request, config, identity, "identity.session.rotate");
    const session = await identity.rotateSession(
      context.organizationId,
      context.claims.sessionId,
      context.claims.subject,
      context.claims.authenticationMethods,
      new Date(Date.now() + 60 * 60 * 1000).toISOString(),
      context.correlationId
    );
    const membership = await identity.getMembership(context.claims.subject, context.organizationId);
    return sessionTokenResponse(membership, session, config);
  });

  app.post<{ Params: { sessionId: string } }>("/v1/auth/sessions/:sessionId/revoke", async (request) => {
    const context = await requestContext(request, config, identity, "identity.session.revoke");
    return identity.revokeSession(
      context.organizationId,
      request.params.sessionId,
      context.claims.subject,
      context.claims.subject,
      context.correlationId
    );
  });

  app.get("/v1/auth/mfa/methods", async (request) => {
    const context = await requestContext(request, config, identity, "identity.mfa.read");
    return identity.listMfaMethods(context.organizationId, context.claims.subject);
  });

  app.post<{ Body: { method?: MfaMethodSummary["method"]; label?: string } }>(
    "/v1/auth/mfa/methods",
    async (request, reply) => {
      const context = await requestContext(request, config, identity, "identity.mfa.enroll");
      const method = request.body?.method;
      if (method !== "totp" && method !== "webauthn") throw new Error("A valid MFA method is required");
      if (method === "totp") {
        const membership = await identity.getMembership(context.claims.subject, context.organizationId);
        const result = await identity.enrollTotpMethod(
          context.organizationId,
          context.claims.subject,
          request.body?.label,
          membership.email ?? membership.userId,
          "PeopleSyncD Genesis",
          context.correlationId
        );
        return reply.status(201).send(result);
      }
      const record = await identity.enrollMfaMethod(
        context.organizationId,
        context.claims.subject,
        method,
        request.body?.label,
        context.correlationId
      );
      return reply.status(201).send(record);
    }
  );

  app.post<{ Params: { methodId: string }; Body: { code?: string } }>(
    "/v1/auth/mfa/totp/:methodId/verify",
    async (request) => {
      const context = await requestContext(request, config, identity, "identity.mfa.verify");
      const code = request.body?.code?.trim();
      if (!code) throw new Error("MFA code is required");
      const result = await identity.verifyTotpMethod(
        context.organizationId,
        context.claims.subject,
        request.params.methodId,
        code,
        context.claims.sessionId,
        new Date(Date.now() + 60 * 60 * 1000).toISOString(),
        context.correlationId
      );
      const membership = await identity.getMembership(context.claims.subject, context.organizationId);
      return {
        ...sessionTokenResponse(membership, result.session, config),
        method: result.method,
        recoveryCodes: result.recoveryCodes
      };
    }
  );

  app.get("/v1/organizations/memberships", async (request) => {
    const context = await requestContext(request, config, identity, "organization.membership.read");
    return identity.listMemberships(context.organizationId);
  });

  app.patch<{
    Params: { userId: string };
    Body: { status?: OrganizationMembershipSummary["status"]; permissions?: unknown };
  }>("/v1/organizations/memberships/:userId", async (request) => {
    const context = await requestContext(request, config, identity, "organization.membership.manage");
    const status = request.body?.status;
    if (status !== "active" && status !== "suspended" && status !== "ended") {
      throw new Error("A valid membership status is required");
    }
    return identity.updateMembership(
      context.organizationId,
      request.params.userId,
      status,
      parsePermissions(request.body?.permissions),
      context.claims.subject,
      context.correlationId
    );
  });

  app.get("/v1/persons", async (request) => {
    const context = await requestContext(request, config, identity, "person.read.summary");
    return store.listPersons(context.organizationId);
  });

  app.post<{ Body: { displayName?: string; preferredName?: string } }>("/v1/persons", async (request, reply) => {
    const context = await requestContext(request, config, identity, "person.create");
    if (!request.body?.displayName?.trim()) throw new Error("displayName is required");
    const person = await store.createPerson(context.organizationId, {
      displayName: request.body.displayName,
      preferredName: request.body.preferredName
    });
    await store.audit.append({
      organizationId: context.organizationId,
      actorId: context.claims.subject,
      action: "person.create",
      resourceType: "person",
      resourceId: person.id,
      outcome: "success",
      correlationId: context.correlationId
    });
    return reply.status(201).send(person);
  });

  app.get("/v1/workers", async (request) => {
    const context = await requestContext(request, config, identity, "worker.read");
    return store.listWorkers(context.organizationId);
  });

  app.post<{ Body: Partial<Omit<WorkerSummary, "id" | "organizationId" | "rowVersion">> }>("/v1/workers", async (request, reply) => {
    const context = await requestContext(request, config, identity, "worker.create");
    const body = request.body ?? {};
    if (!body.personId || !body.workerType || !body.employmentStatus || !body.startDate) {
      throw new Error("personId, workerType, employmentStatus, and startDate are required");
    }
    const worker = await store.createWorker(context.organizationId, {
      personId: body.personId,
      workerType: body.workerType,
      employmentStatus: body.employmentStatus,
      startDate: body.startDate
    });
    await store.audit.append({
      organizationId: context.organizationId,
      actorId: context.claims.subject,
      action: "worker.create",
      resourceType: "worker",
      resourceId: worker.id,
      outcome: "success",
      correlationId: context.correlationId
    });
    return reply.status(201).send(worker);
  });

  app.post<{ Body: { action?: string; resourceType?: string; resourceId?: string; outcome?: "success" | "denied" | "failure" } }>("/v1/audit/events", async (request, reply) => {
    const context = await requestContext(request, config, identity, "audit.append");
    if (!request.body?.action || !request.body.resourceType || !request.body.outcome) {
      throw new Error("action, resourceType, and outcome are required");
    }
    const event = await store.audit.append({
      organizationId: context.organizationId,
      actorId: context.claims.subject,
      action: request.body.action,
      resourceType: request.body.resourceType,
      resourceId: request.body.resourceId,
      outcome: request.body.outcome,
      correlationId: context.correlationId
    });
    return reply.status(202).send(event);
  });

  app.get("/v1/audit/events", async (request) => {
    const context = await requestContext(request, config, identity, "audit.read");
    return store.audit.list(context.organizationId, 50);
  });

  app.get("/v1/founder/dashboard", async (request) => {
    const context = await requestContext(request, config, identity, "founder.dashboard.read");
    return store.dashboard(context.organizationId);
  });

  app.get("/v1/ai/tools", async (request) => {
    await requestContext(request, config, identity, "ai.tool.founder.get_brief");
    return [{
      id: "founder.get_brief",
      version: "1.0.0",
      risk: "read_only",
      approvalRequired: false,
      description: "Returns a source-linked Founder Brief from authorized platform data."
    }];
  });

  app.post<{ Params: { toolId: string } }>("/v1/ai/tools/:toolId/invoke", async (request, reply) => {
    const context = await requestContext(request, config, identity, "ai.tool.founder.get_brief");
    if (request.params.toolId !== "founder.get_brief") return reply.status(404).send({ error: "Tool not registered" });
    const result = await store.dashboard(context.organizationId);
    await store.audit.append({
      organizationId: context.organizationId,
      actorId: context.claims.subject,
      action: "ai.tool.invoke",
      resourceType: "ai_tool",
      resourceId: request.params.toolId,
      outcome: "success",
      correlationId: context.correlationId,
      metadata: { risk: "read_only" }
    });
    return {
      toolId: request.params.toolId,
      risk: "read_only",
      approvalRequired: false,
      result,
      sources: [
        { type: "platform", reference: "/v1/founder/dashboard" },
        { type: "governance", reference: "docs/ui/founder-dashboard.md" }
      ],
      generatedAt: new Date().toISOString()
    };
  });

  return app;
}

import Fastify from "fastify";
import cors from "@fastify/cors";
import { createSessionToken } from "@peoplesyncd/auth";
import { AuthorizationError } from "@peoplesyncd/permissions";
import { GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import type { Permission, SessionClaims, WorkerSummary } from "@peoplesyncd/shared";
import type { ApiConfig } from "./config";
import { readConfig } from "./config";
import { requestContext } from "./security";
import { InMemoryPlatformStore } from "./store";

const FOUNDER_PERMISSIONS: Permission[] = [
  "founder.dashboard.read",
  "person.read.summary",
  "person.create",
  "worker.read",
  "worker.create",
  "audit.append",
  "audit.read",
  "ai.tool.founder.get_brief"
];

export function buildServer(config: ApiConfig = readConfig(), store = new InMemoryPlatformStore()) {
  const app = Fastify({ logger: false, trustProxy: false, bodyLimit: 1024 * 1024 });
  void app.register(cors, { origin: config.corsOrigin, credentials: false });

  app.setErrorHandler((error, _request, reply) => {
    if (error instanceof AuthorizationError || /session|bearer|signature|expired/i.test(error.message)) {
      return reply.status(error instanceof AuthorizationError ? 403 : 401).send({ error: error.message });
    }
    if (/not found|invalid|required/i.test(error.message)) {
      return reply.status(400).send({ error: error.message });
    }
    return reply.status(500).send({ error: "Internal server error" });
  });

  app.get("/health/live", async () => ({ status: "live", release: "0.2.0-internal-alpha" }));
  app.get("/health/ready", async () => ({ status: "ready", storage: "in-memory", certified: false }));

  app.post("/v1/auth/dev-session", async (_request, reply) => {
    if (!config.devAuthEnabled) return reply.status(404).send({ error: "Not found" });
    const now = Math.floor(Date.now() / 1000);
    const claims: SessionClaims = {
      subject: "founder-jason",
      displayName: "Jason Henderson",
      organizationIds: [GENESIS_ORGANIZATION_ID],
      permissions: FOUNDER_PERMISSIONS,
      issuedAt: now,
      expiresAt: now + 60 * 60
    };
    return {
      token: createSessionToken(claims, config.sessionSecret),
      organizationId: GENESIS_ORGANIZATION_ID,
      expiresAt: claims.expiresAt,
      mode: "development-only"
    };
  });

  app.get("/v1/persons", async (request) => {
    const context = requestContext(request, config, "person.read.summary");
    return store.listPersons(context.organizationId);
  });

  app.post<{ Body: { displayName?: string; preferredName?: string } }>("/v1/persons", async (request, reply) => {
    const context = requestContext(request, config, "person.create");
    if (!request.body?.displayName?.trim()) throw new Error("displayName is required");
    const person = store.createPerson(context.organizationId, {
      displayName: request.body.displayName,
      preferredName: request.body.preferredName
    });
    store.audit.append({
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
    const context = requestContext(request, config, "worker.read");
    return store.listWorkers(context.organizationId);
  });

  app.post<{ Body: Partial<Omit<WorkerSummary, "id" | "organizationId" | "rowVersion">> }>("/v1/workers", async (request, reply) => {
    const context = requestContext(request, config, "worker.create");
    const body = request.body ?? {};
    if (!body.personId || !body.workerType || !body.employmentStatus || !body.startDate) {
      throw new Error("personId, workerType, employmentStatus, and startDate are required");
    }
    const worker = store.createWorker(context.organizationId, {
      personId: body.personId,
      workerType: body.workerType,
      employmentStatus: body.employmentStatus,
      startDate: body.startDate
    });
    store.audit.append({
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
    const context = requestContext(request, config, "audit.append");
    if (!request.body?.action || !request.body.resourceType || !request.body.outcome) {
      throw new Error("action, resourceType, and outcome are required");
    }
    const event = store.audit.append({
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
    const context = requestContext(request, config, "audit.read");
    return store.audit.list(context.organizationId, 50);
  });

  app.get("/v1/founder/dashboard", async (request) => {
    const context = requestContext(request, config, "founder.dashboard.read");
    return store.dashboard(context.organizationId);
  });

  app.get("/v1/ai/tools", async (request) => {
    requestContext(request, config, "ai.tool.founder.get_brief");
    return [{
      id: "founder.get_brief",
      version: "1.0.0",
      risk: "read_only",
      approvalRequired: false,
      description: "Returns a source-linked Founder Brief from authorized platform data."
    }];
  });

  app.post<{ Params: { toolId: string } }>("/v1/ai/tools/:toolId/invoke", async (request, reply) => {
    const context = requestContext(request, config, "ai.tool.founder.get_brief");
    if (request.params.toolId !== "founder.get_brief") return reply.status(404).send({ error: "Tool not registered" });
    const result = store.dashboard(context.organizationId);
    store.audit.append({
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

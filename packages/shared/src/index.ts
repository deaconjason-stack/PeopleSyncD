export const GENESIS_ORGANIZATION_ID = "11111111-1111-4111-8111-111111111111";

export const ACTIVE_BOARD = [
  { id: "board-jason", displayName: "Jason Henderson", role: "Founder and Chair" },
  { id: "board-domonique", displayName: "Domonique Danielle Henderson", role: "Co-Founder and Board Member" },
  { id: "board-marietta", displayName: "Marietta Jessup", role: "Board Member" }
] as const;

export type Permission =
  | "founder.dashboard.read"
  | "person.read.summary"
  | "person.create"
  | "worker.read"
  | "worker.create"
  | "audit.append"
  | "audit.read"
  | "ai.tool.founder.get_brief";

export interface SessionClaims {
  subject: string;
  displayName: string;
  organizationIds: string[];
  permissions: Permission[];
  issuedAt: number;
  expiresAt: number;
}

export interface PersonSummary {
  id: string;
  organizationId: string;
  displayName: string;
  preferredName?: string;
  createdAt: string;
}

export interface WorkerSummary {
  id: string;
  organizationId: string;
  personId: string;
  workerType: "employee" | "contractor" | "volunteer" | "intern" | "instructor" | "advisor";
  employmentStatus: "planned" | "onboarding" | "active" | "leave" | "suspended" | "ended" | "archived";
  startDate: string;
  rowVersion: number;
}

export interface AuditEvent {
  id: string;
  organizationId: string;
  actorId: string;
  action: string;
  resourceType: string;
  resourceId?: string;
  outcome: "success" | "denied" | "failure";
  occurredAt: string;
  correlationId: string;
  metadata?: Record<string, string | number | boolean | null>;
}

export interface FounderDashboard {
  organizationId: string;
  activeWorkers: number;
  onboardingWorkers: number;
  people: number;
  pendingApprovals: number;
  board: ReadonlyArray<{ id: string; displayName: string; role: string }>;
  recentAudit: AuditEvent[];
  generatedAt: string;
}

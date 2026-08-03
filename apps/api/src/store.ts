import { randomUUID } from "node:crypto";
import { ACTIVE_BOARD, GENESIS_ORGANIZATION_ID } from "@peoplesyncd/shared";
import type { FounderDashboard, PersonSummary, WorkerSummary } from "@peoplesyncd/shared";
import { InMemoryAuditStore } from "@peoplesyncd/audit";

export class InMemoryPlatformStore {
  readonly audit = new InMemoryAuditStore();
  private readonly persons: PersonSummary[] = [
    {
      id: "22222222-2222-4222-8222-222222222222",
      organizationId: GENESIS_ORGANIZATION_ID,
      displayName: "Alex Morgan",
      preferredName: "Alex",
      createdAt: new Date("2026-08-03T12:00:00Z").toISOString()
    }
  ];
  private readonly workers: WorkerSummary[] = [
    {
      id: "33333333-3333-4333-8333-333333333333",
      organizationId: GENESIS_ORGANIZATION_ID,
      personId: "22222222-2222-4222-8222-222222222222",
      workerType: "employee",
      employmentStatus: "onboarding",
      startDate: "2026-08-10",
      rowVersion: 1
    }
  ];

  listPersons(organizationId: string): PersonSummary[] {
    return this.persons.filter((person) => person.organizationId === organizationId);
  }

  createPerson(organizationId: string, input: { displayName: string; preferredName?: string }): PersonSummary {
    const person: PersonSummary = {
      id: randomUUID(),
      organizationId,
      displayName: input.displayName.trim(),
      preferredName: input.preferredName?.trim() || undefined,
      createdAt: new Date().toISOString()
    };
    this.persons.push(person);
    return person;
  }

  listWorkers(organizationId: string): WorkerSummary[] {
    return this.workers.filter((worker) => worker.organizationId === organizationId);
  }

  createWorker(organizationId: string, input: Omit<WorkerSummary, "id" | "organizationId" | "rowVersion">): WorkerSummary {
    if (!this.persons.some((person) => person.id === input.personId && person.organizationId === organizationId)) {
      throw new Error("Person not found in organization");
    }
    const worker: WorkerSummary = {
      id: randomUUID(),
      organizationId,
      ...input,
      rowVersion: 1
    };
    this.workers.push(worker);
    return worker;
  }

  dashboard(organizationId: string): FounderDashboard {
    const workers = this.listWorkers(organizationId);
    return {
      organizationId,
      activeWorkers: workers.filter((worker) => worker.employmentStatus === "active").length,
      onboardingWorkers: workers.filter((worker) => worker.employmentStatus === "onboarding").length,
      people: this.listPersons(organizationId).length,
      pendingApprovals: 0,
      board: ACTIVE_BOARD,
      recentAudit: this.audit.list(organizationId, 10),
      generatedAt: new Date().toISOString()
    };
  }
}

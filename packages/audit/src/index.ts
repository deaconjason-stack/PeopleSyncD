import { randomUUID } from "node:crypto";
import type { AuditEvent } from "@peoplesyncd/shared";

export class InMemoryAuditStore {
  private readonly events: AuditEvent[] = [];

  append(input: Omit<AuditEvent, "id" | "occurredAt">): AuditEvent {
    const event: AuditEvent = {
      ...input,
      id: randomUUID(),
      occurredAt: new Date().toISOString()
    };
    this.events.push(Object.freeze(event));
    return event;
  }

  list(organizationId: string, limit = 20): AuditEvent[] {
    return this.events
      .filter((event) => event.organizationId === organizationId)
      .slice(-limit)
      .reverse();
  }
}

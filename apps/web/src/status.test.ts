import { describe, expect, it } from "vitest";
import { dashboardStatus } from "./status";
import type { FounderDashboard } from "@peoplesyncd/shared";

const base: FounderDashboard = {
  organizationId: "org",
  activeWorkers: 0,
  onboardingWorkers: 0,
  people: 0,
  pendingApprovals: 0,
  board: [],
  recentAudit: [],
  generatedAt: new Date(0).toISOString()
};

describe("dashboard status", () => {
  it("prioritizes onboarding attention", () => {
    expect(dashboardStatus({ ...base, onboardingWorkers: 1 })).toBe("1 onboarding item needs attention");
  });
});

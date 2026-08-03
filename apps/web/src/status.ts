import type { FounderDashboard } from "@peoplesyncd/shared";

export function dashboardStatus(dashboard: FounderDashboard): string {
  if (dashboard.onboardingWorkers > 0) {
    const count = dashboard.onboardingWorkers;
    return `${count} onboarding item${count === 1 ? "" : "s"} ${count === 1 ? "needs" : "need"} attention`;
  }
  if (dashboard.pendingApprovals > 0) {
    const count = dashboard.pendingApprovals;
    return `${count} approval${count === 1 ? "" : "s"} pending`;
  }
  return "No urgent workforce actions";
}

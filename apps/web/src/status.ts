import type { FounderDashboard } from "@peoplesyncd/shared";

export function dashboardStatus(dashboard: FounderDashboard): string {
  if (dashboard.onboardingWorkers > 0) return `${dashboard.onboardingWorkers} onboarding item${dashboard.onboardingWorkers === 1 ? "" : "s"} need attention`;
  if (dashboard.pendingApprovals > 0) return `${dashboard.pendingApprovals} approval${dashboard.pendingApprovals === 1 ? "" : "s"} pending`;
  return "No urgent workforce actions";
}

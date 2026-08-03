import { useEffect, useMemo, useState } from "react";
import { PeopleSyncDClient } from "@peoplesyncd/sdk";
import type { FounderDashboard, PersonSummary, WorkerSummary } from "@peoplesyncd/shared";
import { dashboardStatus } from "./status";

const API_BASE = import.meta.env.VITE_PEOPLESYNCD_API_URL ?? "http://127.0.0.1:8080";

interface DevSession {
  token: string;
  organizationId: string;
  expiresAt: number;
  mode: string;
}

async function createDevSession(): Promise<DevSession> {
  const response = await fetch(`${API_BASE}/v1/auth/dev-session`, { method: "POST" });
  if (!response.ok) throw new Error("Development session is unavailable. Start the local API with development authentication enabled.");
  return response.json() as Promise<DevSession>;
}

export function App() {
  const [session, setSession] = useState<DevSession | null>(null);
  const [dashboard, setDashboard] = useState<FounderDashboard | null>(null);
  const [persons, setPersons] = useState<PersonSummary[]>([]);
  const [workers, setWorkers] = useState<WorkerSummary[]>([]);
  const [brief, setBrief] = useState<string>("");
  const [error, setError] = useState<string>("");
  const [loading, setLoading] = useState(false);

  const client = useMemo(() => session ? new PeopleSyncDClient({
    baseUrl: API_BASE,
    token: session.token,
    organizationId: session.organizationId
  }) : null, [session]);

  async function loadData(activeClient = client): Promise<void> {
    if (!activeClient) return;
    setLoading(true);
    setError("");
    try {
      const [nextDashboard, nextPersons, nextWorkers] = await Promise.all([
        activeClient.founderDashboard(),
        activeClient.persons(),
        activeClient.workers()
      ]);
      setDashboard(nextDashboard);
      setPersons(nextPersons);
      setWorkers(nextWorkers);
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Unable to load PeopleSyncD data");
    } finally {
      setLoading(false);
    }
  }

  async function signIn(): Promise<void> {
    setLoading(true);
    setError("");
    try {
      const nextSession = await createDevSession();
      setSession(nextSession);
      const nextClient = new PeopleSyncDClient({ baseUrl: API_BASE, token: nextSession.token, organizationId: nextSession.organizationId });
      await loadData(nextClient);
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Unable to start development session");
      setLoading(false);
    }
  }

  async function generateFounderBrief(): Promise<void> {
    if (!session) return;
    setLoading(true);
    setError("");
    try {
      const response = await fetch(`${API_BASE}/v1/ai/tools/founder.get_brief/invoke`, {
        method: "POST",
        headers: {
          authorization: `Bearer ${session.token}`,
          "x-organization-id": session.organizationId
        }
      });
      if (!response.ok) throw new Error(await response.text());
      const payload = await response.json() as { result: FounderDashboard; sources: Array<{ reference: string }> };
      setBrief(`${dashboardStatus(payload.result)}. Sources: ${payload.sources.map((source) => source.reference).join(", ")}`);
      await loadData();
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : "Unable to generate Founder Brief");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (session && !dashboard) void loadData();
  }, [session]);

  if (!session) {
    return (
      <main id="main" className="signin-shell">
        <section className="signin-card" aria-labelledby="signin-title">
          <p className="eyebrow">PeopleSyncD Enterprise Platform</p>
          <h1 id="signin-title">Genesis Internal Alpha</h1>
          <p>Launch the protected founder demonstration using the local development API. This mode is disabled in production.</p>
          <button type="button" onClick={() => void signIn()} disabled={loading}>{loading ? "Connecting…" : "Open Founder Workspace"}</button>
          {error && <p className="error" role="alert">{error}</p>}
          <p className="truth">Internal, unsigned, and not production certified.</p>
        </section>
      </main>
    );
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">PeopleSyncD</p>
          <h1>Founder Dashboard</h1>
        </div>
        <div className="topbar-actions">
          <span className="status-chip">Genesis 0.2.0 Internal Alpha</span>
          <button type="button" className="secondary" onClick={() => void loadData()} disabled={loading}>Refresh</button>
        </div>
      </header>

      <main id="main" className="workspace">
        {error && <p className="error" role="alert">{error}</p>}
        <section className="hero-panel" aria-labelledby="welcome-title">
          <div>
            <p className="eyebrow">Welcome, Jason</p>
            <h2 id="welcome-title">Build the company with clarity and care.</h2>
            <p>{dashboard ? dashboardStatus(dashboard) : "Loading your authorized company view…"}</p>
          </div>
          <button type="button" onClick={() => void generateFounderBrief()} disabled={loading}>Ask Domonique for Founder Brief</button>
        </section>

        {brief && <section className="brief" aria-live="polite"><strong>Domonique 2.0:</strong> {brief}</section>}

        <section className="metrics" aria-label="Company metrics">
          <article><span>People</span><strong>{dashboard?.people ?? "—"}</strong></article>
          <article><span>Active workers</span><strong>{dashboard?.activeWorkers ?? "—"}</strong></article>
          <article><span>Onboarding</span><strong>{dashboard?.onboardingWorkers ?? "—"}</strong></article>
          <article><span>Approvals</span><strong>{dashboard?.pendingApprovals ?? "—"}</strong></article>
        </section>

        <section className="grid-two">
          <article className="panel">
            <h2>Authoritative Board</h2>
            <ul className="board-list">
              {dashboard?.board.map((member) => <li key={member.id}><strong>{member.displayName}</strong><span>{member.role}</span></li>)}
            </ul>
          </article>
          <article className="panel">
            <h2>Recent Audit Evidence</h2>
            {dashboard?.recentAudit.length ? <ul className="audit-list">{dashboard.recentAudit.map((event) => <li key={event.id}><span>{event.action}</span><time>{new Date(event.occurredAt).toLocaleString()}</time></li>)}</ul> : <p className="empty">No recorded actions yet.</p>}
          </article>
        </section>

        <section className="grid-two">
          <article className="panel">
            <h2>People Directory</h2>
            <ul className="simple-list">{persons.map((person) => <li key={person.id}><strong>{person.displayName}</strong><span>{person.preferredName ?? "No preferred name"}</span></li>)}</ul>
          </article>
          <article className="panel">
            <h2>Worker Lifecycle</h2>
            <ul className="simple-list">{workers.map((worker) => <li key={worker.id}><strong>{worker.workerType}</strong><span>{worker.employmentStatus} · starts {worker.startDate}</span></li>)}</ul>
          </article>
        </section>
      </main>
    </div>
  );
}

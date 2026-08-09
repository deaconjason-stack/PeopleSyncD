import "./page.css";

const modules = [
  ["People", "Workforce profiles, teams, roles, and organizational relationships."],
  ["Organizations", "Manage tenants, business units, departments, and governance."],
  ["Documents", "Secure enterprise documents with permissions and audit history."],
  ["Onboarding", "Coordinate repeatable employee and organization workflows."],
  ["Audit", "Trace important activity across the platform."],
  ["Domonique", "AI-assisted enterprise work with governed organizational context."],
];

const stats = [
  ["PEOPLE", "—", "Live data loading"],
  ["ORGANIZATIONS", "—", "Live data loading"],
  ["DOCUMENTS", "—", "Coming online"],
  ["AUDIT EVENTS", "—", "Coming online"],
];

export default function Home() {
  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div className="logo">PeopleSyncD</div>
        <div className="workspace">YOUR ORGANIZATION <span>⌄</span></div>
        <nav className="side-nav">
          <a className="active" href="#dashboard">⌂ <span>Dashboard</span></a>
          <a href="/people">♙ <span>People</span></a>
          <a href="#organizations">▦ <span>Organizations</span></a>
          <a href="#documents">▤ <span>Documents</span></a>
          <a href="#onboarding">✓ <span>Onboarding</span></a>
          <a href="#audit">◷ <span>Audit</span></a>
          <a href="#domonique">✦ <span>Domonique</span></a>
        </nav>
        <div className="sidebar-bottom">
          <a href="#settings">⚙ <span>Administration</span></a>
          <div className="user-card"><div className="avatar">PS</div><div><strong>PeopleSyncD User</strong><small>Authenticated workspace</small></div></div>
        </div>
      </aside>

      <section className="main-area">
        <header className="topbar">
          <div className="breadcrumb">PeopleSyncD / <strong>Dashboard</strong></div>
          <div className="top-actions"><button aria-label="Search">⌕</button><button aria-label="Notifications">♢</button><div className="mini-avatar">PS</div></div>
        </header>

        <div className="content" id="dashboard">
          <div className="welcome"><div><p className="eyebrow">ENTERPRISE COMMAND CENTER</p><h1>Welcome to PeopleSyncD.</h1><p>Your organization workspace is being connected to live platform services.</p></div><a className="primary" href="/people">Open People →</a></div>
          <div className="stats">{stats.map(([label,value,detail])=><article key={label}><span>{label}</span><strong>{value}</strong><small>{detail}</small></article>)}</div>
          <div className="grid">
            <section className="panel domonique-panel" id="domonique">
              <div className="panel-head"><div><p className="eyebrow">DOMONIQUE 2.0</p><h2>What would you like to accomplish?</h2></div><span className="ai-badge">AI</span></div>
              <p className="muted">Ask about authorized organizational information, workflows, documents, or operational priorities.</p>
              <div className="prompt"><span>Ask Domonique anything about your organization...</span><button>→</button></div>
              <div className="suggestions"><button>Give me my founder brief</button><button>Show onboarding activity</button><button>What needs my attention?</button></div>
            </section>
            <section className="panel activity"><div className="panel-head"><div><p className="eyebrow">ACTIVITY</p><h2>Recent events</h2></div><a href="#audit">View all</a></div><div className="event"><span className="event-dot"/><div><strong>Live audit activity will appear here</strong><small>Connected to the PeopleSyncD audit service</small></div></div></section>
          </div>
          <section className="modules"><div className="panel-head"><div><p className="eyebrow">PLATFORM</p><h2>Core modules</h2></div></div><div className="module-grid">{modules.map(([name,text])=><a className="module" href={name === "People" ? "/people" : `#${name.toLowerCase()}`} key={name}><span className="module-icon">{name === "Domonique" ? "✦" : "□"}</span><div><h3>{name}</h3><p>{text}</p></div><span>→</span></a>)}</div></section>
        </div>
      </section>
    </main>
  );
}

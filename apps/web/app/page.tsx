const capabilities = [
  { title: "People Operations", text: "Manage people, organizations, teams, roles, and workforce data from one governed platform." },
  { title: "Enterprise Knowledge", text: "Connect policies, documents, organizational knowledge, and operational context." },
  { title: "Domonique Intelligence", text: "Give authorized teams an AI assistant designed around enterprise context and controls." },
  { title: "Governance & Audit", text: "Make important actions traceable with permissions, audit history, and enterprise controls." },
];

export default function Home() {
  return (
    <main>
      <header className="site-header">
        <div className="container nav">
          <div className="brand">PeopleSyncD</div>
          <nav>
            <a href="#platform">Platform</a>
            <a href="#intelligence">Domonique</a>
            <a href="#enterprise">Enterprise</a>
            <a href="#contact">Contact</a>
          </nav>
          <a className="nav-button" href="#contact">Request a demo</a>
        </div>
      </header>

      <section className="hero">
        <div className="container hero-grid">
          <div>
            <p className="eyebrow">THE ENTERPRISE PEOPLE OPERATING PLATFORM</p>
            <h1>Connect your people, operations, knowledge, and intelligence.</h1>
            <p className="hero-copy">
              PeopleSyncD is being built as a unified enterprise platform for managing organizational data,
              workflows, knowledge, governance, and AI-assisted operations.
            </p>
            <div className="actions">
              <a className="primary" href="#contact">Explore PeopleSyncD</a>
              <a className="secondary" href="#platform">See the platform</a>
            </div>
          </div>
          <div className="hero-card">
            <div className="card-label">PLATFORM STATUS</div>
            <div className="status"><span /> Foundation build active</div>
            <div className="metric"><strong>01</strong><span>Unified enterprise foundation</span></div>
            <div className="metric"><strong>AI</strong><span>Domonique intelligence layer</span></div>
            <div className="metric"><strong>∞</strong><span>Designed to scale with the organization</span></div>
          </div>
        </div>
      </section>

      <section id="platform" className="section">
        <div className="container">
          <p className="eyebrow">ONE PLATFORM</p>
          <h2>Built around the organization—not disconnected applications.</h2>
          <div className="capabilities">
            {capabilities.map((item) => (
              <article className="capability" key={item.title}>
                <h3>{item.title}</h3>
                <p>{item.text}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section id="intelligence" className="dark-section">
        <div className="container intelligence">
          <div>
            <p className="eyebrow">DOMONIQUE</p>
            <h2>Enterprise intelligence with context, permissions, and accountability.</h2>
          </div>
          <p>
            Domonique is the planned PeopleSyncD intelligence layer: an AI interface capable of working with
            authorized enterprise knowledge and workflows while preserving organizational controls and auditability.
          </p>
        </div>
      </section>

      <section id="enterprise" className="section">
        <div className="container enterprise">
          <p className="eyebrow">ENTERPRISE READY BY DESIGN</p>
          <h2>Security, governance, integration, and scale belong in the foundation.</h2>
          <p>
            The platform is being engineered with multi-tenancy, identity, auditability, observability, API contracts,
            infrastructure automation, and deployment portability as first-class concerns.
          </p>
        </div>
      </section>

      <section id="contact" className="cta">
        <div className="container">
          <p className="eyebrow">PEOPLESYNCD</p>
          <h2>The enterprise operating platform is being built now.</h2>
          <p>Follow the platform as the foundation becomes a working product.</p>
          <a className="primary" href="https://github.com/deaconjason-stack/PeopleSyncD">View the build on GitHub</a>
        </div>
      </section>

      <footer>
        <div className="container footer-inner">
          <span>© 2026 PeopleSyncD</span>
          <span>Enterprise Platform · Foundation Build</span>
        </div>
      </footer>
    </main>
  );
}

const capabilities = [
  'Tenant-aware organizations and identity',
  'Clean Architecture application services',
  'PostgreSQL and EF Core persistence',
  'OpenAPI-first REST foundation',
  'OpenTelemetry and Aspire orchestration',
];

export default function HomePage() {
  return (
    <main>
      <section className="hero" aria-labelledby="page-title">
        <p className="eyebrow">Genesis · Milestone M1.2.1</p>
        <h1 id="page-title">PeopleSyncD is becoming a buildable platform.</h1>
        <p className="lede">
          This Next.js shell is connected to the new .NET enterprise solution foundation. Feature development follows tested vertical slices from this point forward.
        </p>
      </section>
      <section aria-labelledby="foundation-title">
        <h2 id="foundation-title">Foundation capabilities</h2>
        <ul>
          {capabilities.map((capability) => <li key={capability}>{capability}</li>)}
        </ul>
      </section>
    </main>
  );
}

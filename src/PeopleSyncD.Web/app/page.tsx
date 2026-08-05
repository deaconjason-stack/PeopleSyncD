import Link from 'next/link';

const capabilities = [
  'Owner and organization registration',
  'Password authentication through ASP.NET Core Identity',
  'Tenant selection and organization membership checks',
  'Short-lived permission-bearing access tokens',
  'Tenant-bound protected organization APIs',
];

export default function HomePage() {
  return (
    <main>
      <section className="hero" aria-labelledby="page-title">
        <p className="eyebrow">Genesis · Milestone M2.1</p>
        <h1 id="page-title">PeopleSyncD now has a working identity and tenant boundary.</h1>
        <p className="lede">
          The platform can create its first organization owner, authenticate users, select authorized organizations, and enforce tenant-scoped permissions through the .NET API.
        </p>
        <Link className="primary-link" href="/auth">Open identity workspace</Link>
      </section>
      <section aria-labelledby="foundation-title">
        <h2 id="foundation-title">Verified vertical-slice capabilities</h2>
        <ul>
          {capabilities.map((capability) => <li key={capability}>{capability}</li>)}
        </ul>
      </section>
    </main>
  );
}

import Link from 'next/link';
import SecurityWorkspace from './SecurityWorkspace';

export default function SecurityPage() {
  return (
    <main>
      <section className="hero compact">
        <p className="eyebrow">M2.3 security center</p>
        <h1>Protect your PeopleSyncD account.</h1>
        <p className="lede">Enroll an authenticator, protect recovery codes, and control active sign-in sessions.</p>
        <Link className="secondary-link" href="/auth">Back to platform access</Link>
      </section>
      <SecurityWorkspace />
    </main>
  );
}

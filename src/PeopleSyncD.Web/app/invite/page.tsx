import Link from 'next/link';
import InviteAcceptance from './InviteAcceptance';

export default function InvitePage() {
  return (
    <main>
      <section className="hero compact">
        <p className="eyebrow">Secure onboarding</p>
        <h1>Accept a PeopleSyncD organization invitation.</h1>
        <p className="lede">Invitation tokens are single-use and expire after seven days.</p>
        <Link className="secondary-link" href="/auth">Back to sign in</Link>
      </section>
      <InviteAcceptance />
    </main>
  );
}

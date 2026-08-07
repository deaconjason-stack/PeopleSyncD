import Link from 'next/link';
import MembersWorkspace from './MembersWorkspace';

export default function MembersPage() {
  return (
    <main>
      <section className="hero compact">
        <p className="eyebrow">M2.2 · Organization administration</p>
        <h1>Manage people without weakening tenant boundaries.</h1>
        <p className="lede">Invite users, review account security, change roles, and suspend or revoke access.</p>
        <Link className="secondary-link" href="/auth">Back to platform access</Link>
      </section>
      <MembersWorkspace />
    </main>
  );
}

import AuthWorkspace from './AuthWorkspace';

export default function AuthPage() {
  return (
    <main>
      <section className="hero compact" aria-labelledby="auth-title">
        <p className="eyebrow">Milestone M2.1</p>
        <h1 id="auth-title">Identity and organization access</h1>
        <p className="lede">
          Create the first owner account, sign in, select an authorized organization, and verify the tenant-scoped session.
        </p>
      </section>
      <AuthWorkspace />
    </main>
  );
}

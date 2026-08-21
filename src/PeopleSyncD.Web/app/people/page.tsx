import PeopleWorkspace from './PeopleWorkspace';

export default function PeoplePage() {
  return (
    <main>
      <section className="hero compact" aria-labelledby="page-title">
        <p className="eyebrow">PeopleSyncD · HR Demo</p>
        <h1 id="page-title">Workforce command center.</h1>
        <p className="lede">Create, find, and manage organization-scoped employee records through the real PeopleSyncD API.</p>
      </section>
      <PeopleWorkspace />
    </main>
  );
}

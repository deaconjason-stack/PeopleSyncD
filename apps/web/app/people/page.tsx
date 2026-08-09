import { getPeople } from "../../lib/api";

export default async function PeoplePage({ searchParams }: { searchParams: Promise<{ organizationId?: string }> }) {
  const { organizationId } = await searchParams;
  if (!organizationId) {
    return <main className="route-page"><p className="eyebrow">PEOPLE</p><h1>People</h1><p>Select an organization to view its people.</p></main>;
  }
  try {
    const people = await getPeople(organizationId);
    return <main className="route-page">
      <p className="eyebrow">PEOPLE</p><h1>People</h1><p>Organization directory</p>
      <div className="empty-state"><strong>{people.length} people</strong><span>{people.length === 0 ? "Add your first person to begin building the organization directory." : "Live people records loaded from the PeopleSyncD API."}</span></div>
      {people.map(person => <article key={person.id} className="route-page" style={{ marginTop: 12 }}><strong>{person.firstName} {person.lastName}</strong><div>{person.email}</div><small>{person.status}</small></article>)}
    </main>;
  } catch {
    return <main className="route-page"><p className="eyebrow">PEOPLE</p><h1>People</h1><div className="empty-state"><strong>Unable to load directory</strong><span>Sign in and verify that you have access to this organization.</span></div></main>;
  }
}

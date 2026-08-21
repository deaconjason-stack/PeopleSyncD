import EmployeeWorkspace from './EmployeeWorkspace';

export default async function EmployeePage({ params }: { params: Promise<{ employeeId: string }> }) {
  const { employeeId } = await params;
  return (
    <main>
      <section className="hero compact" aria-labelledby="page-title">
        <p className="eyebrow">PeopleSyncD · Employee</p>
        <h1 id="page-title">Employee lifecycle.</h1>
        <p className="lede">Manage the persistent workforce record without bypassing tenant or lifecycle controls.</p>
      </section>
      <EmployeeWorkspace employeeId={employeeId} />
    </main>
  );
}

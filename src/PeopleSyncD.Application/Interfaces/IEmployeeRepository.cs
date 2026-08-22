using PeopleSyncD.Domain.Employees;

namespace PeopleSyncD.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetAsync(
        Guid tenantId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Employee>> ListAsync(
        Guid tenantId,
        string? search,
        EmploymentStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

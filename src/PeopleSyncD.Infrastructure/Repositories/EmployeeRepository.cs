using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Employees;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Infrastructure.Repositories;

internal sealed class EmployeeRepository(ApplicationDbContext database) : IEmployeeRepository
{
    public Task<Employee?> GetAsync(
        Guid tenantId,
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        database.Employees.SingleOrDefaultAsync(
            employee => employee.OrganizationId == tenantId && employee.Id == employeeId,
            cancellationToken);

    public async Task<IReadOnlyCollection<Employee>> ListAsync(
        Guid tenantId,
        string? search,
        EmploymentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = database.Employees
            .AsNoTracking()
            .Where(employee => employee.OrganizationId == tenantId);

        if (status is not null)
        {
            query = query.Where(employee => employee.Status == status.Value);
        }

        var items = await query.ToListAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            items = items
                .Where(employee =>
                    employee.DisplayName.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                    || employee.Email.Value.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                    || employee.Title.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                    || employee.Department.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return items
            .OrderBy(employee => employee.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default) =>
        await database.Employees.AddAsync(employee, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        database.SaveChangesAsync(cancellationToken);
}

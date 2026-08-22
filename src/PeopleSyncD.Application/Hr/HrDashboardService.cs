using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Employees;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Hr;

public sealed class HrDashboardService(
    IEmployeeRepository employees,
    IOnboardingRepository onboarding,
    IClock clock)
{
    public async Task<HrDashboardDto> GetAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var workforce = await employees.ListAsync(organizationId, null, null, cancellationToken);
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var overdueOnboarding = await onboarding.CountOverdueTasksAsync(
            organizationId,
            today,
            cancellationToken);

        return new HrDashboardDto(
            workforce.Count,
            workforce.Count(employee => employee.Status == EmploymentStatus.Active),
            workforce.Count(employee => employee.Status == EmploymentStatus.Onboarding),
            workforce.Count(employee => employee.Status == EmploymentStatus.Leave),
            0,
            overdueOnboarding,
            0,
            []);
    }
}

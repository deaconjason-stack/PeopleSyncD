using PeopleSyncD.Domain.Onboarding;

namespace PeopleSyncD.Application.Interfaces;

public interface IOnboardingRepository
{
    Task<EmployeeOnboarding?> GetAsync(
        Guid tenantId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<OnboardingTemplate?> GetActiveTemplateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task AddTemplateAsync(
        OnboardingTemplate template,
        CancellationToken cancellationToken = default);

    Task AddOnboardingAsync(
        EmployeeOnboarding onboarding,
        CancellationToken cancellationToken = default);

    Task<int> CountOverdueTasksAsync(
        Guid tenantId,
        DateOnly today,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

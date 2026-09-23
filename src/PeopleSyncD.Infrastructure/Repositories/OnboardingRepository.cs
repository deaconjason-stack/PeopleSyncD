using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Onboarding;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Infrastructure.Repositories;

internal sealed class OnboardingRepository(ApplicationDbContext database) : IOnboardingRepository
{
    public Task<EmployeeOnboarding?> GetAsync(
        Guid tenantId,
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        database.EmployeeOnboardings
            .Include(item => item.Tasks)
            .SingleOrDefaultAsync(
                item => item.OrganizationId == tenantId && item.EmployeeId == employeeId,
                cancellationToken);

    public Task<OnboardingTemplate?> GetActiveTemplateAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        database.OnboardingTemplates
            .Include(onboardingTemplate => onboardingTemplate.Tasks)
            .Where(onboardingTemplate => onboardingTemplate.OrganizationId == tenantId && onboardingTemplate.IsActive)
            .OrderByDescending(onboardingTemplate => onboardingTemplate.Version)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddTemplateAsync(
        OnboardingTemplate onboardingTemplate,
        CancellationToken cancellationToken = default) =>
        await database.OnboardingTemplates.AddAsync(onboardingTemplate, cancellationToken);

    public async Task AddOnboardingAsync(
        EmployeeOnboarding onboarding,
        CancellationToken cancellationToken = default) =>
        await database.EmployeeOnboardings.AddAsync(onboarding, cancellationToken);

    public async Task<int> CountOverdueTasksAsync(
        Guid tenantId,
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var items = await database.EmployeeOnboardings
            .AsNoTracking()
            .Include(item => item.Tasks)
            .Where(item => item.OrganizationId == tenantId)
            .ToListAsync(cancellationToken);

        return items
            .SelectMany(item => item.Tasks)
            .Count(task =>
                task.DueDate < today
                && task.Status is not OnboardingTaskStatus.Completed
                && task.Status is not OnboardingTaskStatus.Waived);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        database.SaveChangesAsync(cancellationToken);
}

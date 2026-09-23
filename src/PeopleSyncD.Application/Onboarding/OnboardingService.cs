using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Onboarding;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Onboarding;

public sealed class OnboardingService(
    IOnboardingRepository onboarding,
    IEmployeeRepository employees,
    IAuditRecorder audit,
    IClock clock)
{
    public async Task<Result<EmployeeOnboardingDto>> GetOrCreateAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await employees.GetAsync(organizationId, employeeId, cancellationToken);
        if (employee is null)
        {
            return NotFound();
        }

        var existing = await onboarding.GetAsync(organizationId, employeeId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(ToDto(existing));
        }

        var template = await onboarding.GetActiveTemplateAsync(organizationId, cancellationToken);
        if (template is null)
        {
            var templateResult = OnboardingTemplate.CreateStandard(organizationId, 1);
            if (templateResult.IsFailure)
            {
                return Result.Failure<EmployeeOnboardingDto>(templateResult.Error);
            }

            template = templateResult.Value;
            await onboarding.AddTemplateAsync(template, cancellationToken);
        }

        var creation = EmployeeOnboarding.Instantiate(template, employee.Id, employee.StartDate);
        if (creation.IsFailure)
        {
            return Result.Failure<EmployeeOnboardingDto>(creation.Error);
        }

        await onboarding.AddOnboardingAsync(creation.Value, cancellationToken);
        await onboarding.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(creation.Value));
    }

    public async Task<Result<EmployeeOnboardingDto>> UpdateTaskAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid employeeId,
        Guid taskId,
        UpdateOnboardingTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var current = await GetOrCreateEntityAsync(organizationId, employeeId, cancellationToken);
        if (current.IsFailure)
        {
            return Result.Failure<EmployeeOnboardingDto>(current.Error);
        }

        if (request.Status != OnboardingTaskStatus.Completed)
        {
            return Result.Failure<EmployeeOnboardingDto>(new DomainError(
                "onboarding.status_not_supported",
                "This onboarding increment currently supports completing checklist tasks."));
        }

        var update = current.Value.CompleteTask(taskId, clock.UtcNow, request.Note);
        if (update.IsFailure)
        {
            return Result.Failure<EmployeeOnboardingDto>(update.Error);
        }

        await onboarding.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(new SecurityAuditEvent(
            "onboarding.task.changed",
            actorUserId,
            organizationId,
            "employee",
            employeeId.ToString("D"),
            clock.UtcNow,
            new Dictionary<string, string>
            {
                ["task_id"] = taskId.ToString("D"),
                ["status"] = request.Status.ToString(),
            }), cancellationToken);

        return Result.Success(ToDto(current.Value));
    }

    private async Task<Result<EmployeeOnboarding>> GetOrCreateEntityAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var employee = await employees.GetAsync(organizationId, employeeId, cancellationToken);
        if (employee is null)
        {
            return Result.Failure<EmployeeOnboarding>(new DomainError(
                "onboarding.employee_not_found",
                "The employee was not found."));
        }

        var existing = await onboarding.GetAsync(organizationId, employeeId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing);
        }

        var template = await onboarding.GetActiveTemplateAsync(organizationId, cancellationToken);
        if (template is null)
        {
            var templateResult = OnboardingTemplate.CreateStandard(organizationId, 1);
            if (templateResult.IsFailure)
            {
                return Result.Failure<EmployeeOnboarding>(templateResult.Error);
            }

            template = templateResult.Value;
            await onboarding.AddTemplateAsync(template, cancellationToken);
        }

        var creation = EmployeeOnboarding.Instantiate(template, employee.Id, employee.StartDate);
        if (creation.IsFailure)
        {
            return creation;
        }

        await onboarding.AddOnboardingAsync(creation.Value, cancellationToken);
        await onboarding.SaveChangesAsync(cancellationToken);
        return creation;
    }

    private static Result<EmployeeOnboardingDto> NotFound() =>
        Result.Failure<EmployeeOnboardingDto>(new DomainError(
            "onboarding.employee_not_found",
            "The employee was not found."));

    private static EmployeeOnboardingDto ToDto(EmployeeOnboarding item) => new(
        item.Id,
        item.OrganizationId,
        item.EmployeeId,
        item.TemplateId,
        item.TemplateName,
        item.TemplateVersion,
        item.StartDate,
        item.CompletedTaskCount,
        item.ProgressPercent,
        item.Tasks
            .OrderBy(task => task.Order)
            .Select(task => new OnboardingTaskDto(
                task.Id,
                task.Title,
                task.Category,
                task.Order,
                task.DueDate,
                task.Status,
                task.CompletedAt,
                task.Note))
            .ToArray());
}

using PeopleSyncD.Domain.Onboarding;

namespace PeopleSyncD.Application.Onboarding;

public sealed record UpdateOnboardingTaskRequest(OnboardingTaskStatus Status, string? Note);

public sealed record OnboardingTaskDto(
    Guid Id,
    string Title,
    string Category,
    int Order,
    DateOnly DueDate,
    OnboardingTaskStatus Status,
    DateTimeOffset? CompletedAt,
    string? Note);

public sealed record EmployeeOnboardingDto(
    Guid Id,
    Guid OrganizationId,
    Guid EmployeeId,
    Guid TemplateId,
    string TemplateName,
    int TemplateVersion,
    DateOnly StartDate,
    int CompletedTaskCount,
    int ProgressPercent,
    IReadOnlyCollection<OnboardingTaskDto> Tasks);

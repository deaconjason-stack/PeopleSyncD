using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Onboarding;

public sealed class OnboardingTask : Entity<Guid>
{
    private OnboardingTask()
    {
        Title = string.Empty;
        Category = string.Empty;
        Note = null;
    }

    internal OnboardingTask(
        Guid id,
        string title,
        string category,
        int order,
        DateOnly dueDate)
        : base(id)
    {
        Title = title;
        Category = category;
        Order = order;
        DueDate = dueDate;
        Status = OnboardingTaskStatus.NotStarted;
    }

    public string Title { get; private set; }

    public string Category { get; private set; }

    public int Order { get; private set; }

    public DateOnly DueDate { get; private set; }

    public OnboardingTaskStatus Status { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? Note { get; private set; }

    internal Result Complete(DateTimeOffset completedAt, string? note)
    {
        if (Status == OnboardingTaskStatus.Waived)
        {
            return Result.Failure(new DomainError(
                "onboarding.task_waived",
                "A waived onboarding task cannot be completed."));
        }

        if (completedAt == default)
        {
            return Result.Failure(new DomainError(
                "onboarding.completion_time_required",
                "Completion time is required."));
        }

        Status = OnboardingTaskStatus.Completed;
        CompletedAt = completedAt;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        return Result.Success();
    }
}

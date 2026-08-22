using PeopleSyncD.Domain.Common;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Onboarding;

public sealed class OnboardingTemplateTask : Entity<Guid>
{
    private OnboardingTemplateTask()
    {
        Title = string.Empty;
        Category = string.Empty;
    }

    internal OnboardingTemplateTask(Guid id, string title, string category, int order, int dueOffsetDays)
        : base(id)
    {
        Title = title;
        Category = category;
        Order = order;
        DueOffsetDays = dueOffsetDays;
    }

    public string Title { get; private set; }

    public string Category { get; private set; }

    public int Order { get; private set; }

    public int DueOffsetDays { get; private set; }

    internal static OnboardingTemplateTask Create(string title, string category, int order, int dueOffsetDays)
    {
        var normalizedTitle = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 200);
        var normalizedCategory = Guard.AgainstNullOrWhiteSpace(category, nameof(category), 80);
        return new OnboardingTemplateTask(Guid.NewGuid(), normalizedTitle, normalizedCategory, order, dueOffsetDays);
    }
}

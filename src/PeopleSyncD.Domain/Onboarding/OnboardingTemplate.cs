using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Onboarding;

public sealed class OnboardingTemplate : AggregateRoot<Guid>
{
    private readonly List<OnboardingTemplateTask> _tasks = [];

    private OnboardingTemplate()
    {
        Name = string.Empty;
    }

    private OnboardingTemplate(Guid id, Guid organizationId, string name, int version, bool isActive)
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Version = version;
        IsActive = isActive;
    }

    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; }

    public int Version { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<OnboardingTemplateTask> Tasks => _tasks.AsReadOnly();

    public static Result<OnboardingTemplate> CreateStandard(Guid organizationId, int version)
    {
        if (organizationId == Guid.Empty)
        {
            return Result.Failure<OnboardingTemplate>(new DomainError(
                "onboarding.organization_required",
                "Organization is required."));
        }

        if (version < 1)
        {
            return Result.Failure<OnboardingTemplate>(new DomainError(
                "onboarding.version_invalid",
                "Template version must be at least 1."));
        }

        var template = new OnboardingTemplate(
            Guid.NewGuid(),
            organizationId,
            "Standard Employee Onboarding",
            version,
            true);

        template._tasks.AddRange(
        [
            OnboardingTemplateTask.Create("Employment Paperwork", "Paperwork", 1, 0),
            OnboardingTemplateTask.Create("Orientation", "Orientation", 2, 1),
            OnboardingTemplateTask.Create("Policy Acknowledgement", "Policy", 3, 2),
            OnboardingTemplateTask.Create("Required Credentials", "Credentials", 4, 3),
            OnboardingTemplateTask.Create("Required Training", "Training", 5, 5),
            OnboardingTemplateTask.Create("Equipment/Access", "Equipment", 6, 1),
            OnboardingTemplateTask.Create("Manager Introduction", "Manager", 7, 1),
        ]);

        return Result.Success(template);
    }
}

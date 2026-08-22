using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Onboarding;

public sealed class EmployeeOnboarding : AggregateRoot<Guid>
{
    private readonly List<OnboardingTask> _tasks = [];

    private EmployeeOnboarding()
    {
        TemplateName = string.Empty;
    }

    private EmployeeOnboarding(
        Guid id,
        Guid organizationId,
        Guid employeeId,
        Guid templateId,
        string templateName,
        int templateVersion,
        DateOnly startDate)
        : base(id)
    {
        OrganizationId = organizationId;
        EmployeeId = employeeId;
        TemplateId = templateId;
        TemplateName = templateName;
        TemplateVersion = templateVersion;
        StartDate = startDate;
    }

    public Guid OrganizationId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public Guid TemplateId { get; private set; }

    public string TemplateName { get; private set; }

    public int TemplateVersion { get; private set; }

    public DateOnly StartDate { get; private set; }

    public IReadOnlyCollection<OnboardingTask> Tasks => _tasks.AsReadOnly();

    public int CompletedTaskCount => _tasks.Count(task => task.Status == OnboardingTaskStatus.Completed);

    public int ProgressPercent
    {
        get
        {
            if (_tasks.Count == 0)
            {
                return 0;
            }

            var done = _tasks.Count(task =>
                task.Status is OnboardingTaskStatus.Completed or OnboardingTaskStatus.Waived);
            return done * 100 / _tasks.Count;
        }
    }

    public static Result<EmployeeOnboarding> Instantiate(
        OnboardingTemplate template,
        Guid employeeId,
        DateOnly startDate)
    {
        if (template is null)
        {
            return Result.Failure<EmployeeOnboarding>(new DomainError(
                "onboarding.template_required",
                "Onboarding template is required."));
        }

        if (employeeId == Guid.Empty)
        {
            return Result.Failure<EmployeeOnboarding>(new DomainError(
                "onboarding.employee_required",
                "Employee is required."));
        }

        if (startDate == default)
        {
            return Result.Failure<EmployeeOnboarding>(new DomainError(
                "onboarding.start_date_required",
                "Start date is required."));
        }

        if (!template.IsActive)
        {
            return Result.Failure<EmployeeOnboarding>(new DomainError(
                "onboarding.template_inactive",
                "An inactive template cannot be instantiated."));
        }

        var onboarding = new EmployeeOnboarding(
            Guid.NewGuid(),
            template.OrganizationId,
            employeeId,
            template.Id,
            template.Name,
            template.Version,
            startDate);

        foreach (var templateTask in template.Tasks.OrderBy(task => task.Order))
        {
            onboarding._tasks.Add(new OnboardingTask(
                Guid.NewGuid(),
                templateTask.Title,
                templateTask.Category,
                templateTask.Order,
                startDate.AddDays(templateTask.DueOffsetDays)));
        }

        return Result.Success(onboarding);
    }

    public Result CompleteTask(Guid taskId, DateTimeOffset completedAt, string? note)
    {
        var task = _tasks.SingleOrDefault(candidate => candidate.Id == taskId);
        return task is null
            ? Result.Failure(new DomainError(
                "onboarding.task_not_found",
                "The onboarding task was not found."))
            : task.Complete(completedAt, note);
    }
}

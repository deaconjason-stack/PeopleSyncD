using PeopleSyncD.Domain.Common;
using PeopleSyncD.Domain.ValueObjects;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Employees;

/// <summary>
/// Employee aggregate scoped to one organization.
/// </summary>
public sealed class Employee : AggregateRoot<Guid>
{
    private Employee()
    {
        EmployeeNumber = string.Empty;
        DisplayName = string.Empty;
        Email = null!;
        Title = string.Empty;
        Department = string.Empty;
        Location = string.Empty;
    }

    private Employee(
        Guid id,
        Guid organizationId,
        string employeeNumber,
        string displayName,
        EmailAddress email,
        string title,
        string department,
        Guid? managerEmployeeId,
        string location,
        EmploymentType employmentType,
        DateOnly startDate)
        : base(id)
    {
        OrganizationId = organizationId;
        EmployeeNumber = employeeNumber;
        DisplayName = displayName;
        Email = email;
        Title = title;
        Department = department;
        ManagerEmployeeId = managerEmployeeId;
        Location = location;
        EmploymentType = employmentType;
        StartDate = startDate;
        Status = EmploymentStatus.Onboarding;
    }

    public Guid OrganizationId { get; private set; }

    public string EmployeeNumber { get; private set; }

    public string DisplayName { get; private set; }

    public EmailAddress Email { get; private set; }

    public string Title { get; private set; }

    public string Department { get; private set; }

    public Guid? ManagerEmployeeId { get; private set; }

    public string Location { get; private set; }

    public EmploymentType EmploymentType { get; private set; }

    public EmploymentStatus Status { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? SeparationDate { get; private set; }

    public static Result<Employee> Create(
        Guid organizationId,
        string? employeeNumber,
        string? displayName,
        string? email,
        string? title,
        string? department,
        Guid? managerEmployeeId,
        string? location,
        EmploymentType employmentType,
        DateOnly startDate)
    {
        if (organizationId == Guid.Empty)
        {
            return Result.Failure<Employee>(new DomainError(
                "employee.organization_required",
                "Organization is required."));
        }

        if (startDate == default)
        {
            return Result.Failure<Employee>(new DomainError(
                "employee.start_date_required",
                "Start date is required."));
        }

        var emailResult = EmailAddress.Create(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<Employee>(emailResult.Error);
        }

        try
        {
            var normalizedEmployeeNumber = Guard.AgainstNullOrWhiteSpace(
                employeeNumber,
                nameof(employeeNumber),
                64);
            var normalizedDisplayName = Guard.AgainstNullOrWhiteSpace(
                displayName,
                nameof(displayName),
                200);
            var normalizedTitle = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 200);
            var normalizedDepartment = Guard.AgainstNullOrWhiteSpace(
                department,
                nameof(department),
                200);
            var normalizedLocation = Guard.AgainstNullOrWhiteSpace(location, nameof(location), 200);

            return Result.Success(new Employee(
                Guid.NewGuid(),
                organizationId,
                normalizedEmployeeNumber,
                normalizedDisplayName,
                emailResult.Value,
                normalizedTitle,
                normalizedDepartment,
                managerEmployeeId,
                normalizedLocation,
                employmentType,
                startDate));
        }
        catch (ArgumentException exception)
        {
            return Result.Failure<Employee>(new DomainError("employee.invalid", exception.Message));
        }
    }

    public Result UpdateProfile(
        string? displayName,
        string? email,
        string? title,
        string? department,
        Guid? managerEmployeeId,
        string? location,
        EmploymentType employmentType)
    {
        if (managerEmployeeId == Id)
        {
            return Result.Failure(new DomainError(
                "employee.manager_self_reference",
                "An employee cannot be their own manager."));
        }

        var emailResult = EmailAddress.Create(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        try
        {
            DisplayName = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName), 200);
            Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 200);
            Department = Guard.AgainstNullOrWhiteSpace(department, nameof(department), 200);
            Location = Guard.AgainstNullOrWhiteSpace(location, nameof(location), 200);
            Email = emailResult.Value;
            ManagerEmployeeId = managerEmployeeId;
            EmploymentType = employmentType;
            return Result.Success();
        }
        catch (ArgumentException exception)
        {
            return Result.Failure(new DomainError("employee.invalid", exception.Message));
        }
    }

    public Result Activate() => Transition(
        EmploymentStatus.Onboarding,
        EmploymentStatus.Active);

    public Result PlaceOnLeave() => Transition(
        EmploymentStatus.Active,
        EmploymentStatus.Leave);

    public Result ReturnFromLeave() => Transition(
        EmploymentStatus.Leave,
        EmploymentStatus.Active);

    public Result Suspend() => Transition(
        EmploymentStatus.Active,
        EmploymentStatus.Suspended);

    public Result Separate(DateOnly separationDate)
    {
        if (Status is EmploymentStatus.Separated or EmploymentStatus.Archived)
        {
            return InvalidTransition(EmploymentStatus.Separated);
        }

        if (separationDate < StartDate)
        {
            return Result.Failure(new DomainError(
                "employee.invalid_separation_date",
                "Separation date cannot be before the employee start date."));
        }

        Status = EmploymentStatus.Separated;
        SeparationDate = separationDate;
        return Result.Success();
    }

    public Result Archive() => Transition(
        EmploymentStatus.Separated,
        EmploymentStatus.Archived);

    private Result Transition(EmploymentStatus requiredCurrent, EmploymentStatus next)
    {
        if (Status != requiredCurrent)
        {
            return InvalidTransition(next);
        }

        Status = next;
        return Result.Success();
    }

    private Result InvalidTransition(EmploymentStatus target) =>
        Result.Failure(new DomainError(
            "employee.invalid_transition",
            $"Employee status cannot change from {Status} to {target}."));
}

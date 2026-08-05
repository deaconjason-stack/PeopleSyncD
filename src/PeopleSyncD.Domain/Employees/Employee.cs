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
        DisplayName = string.Empty;
        Email = null!;
    }

    private Employee(Guid id, Guid organizationId, string displayName, EmailAddress email)
        : base(id)
    {
        OrganizationId = organizationId;
        DisplayName = displayName;
        Email = email;
    }

    public Guid OrganizationId { get; private set; }

    public string DisplayName { get; private set; }

    public EmailAddress Email { get; private set; }

    public static Result<Employee> Create(Guid organizationId, string? displayName, string? email)
    {
        if (organizationId == Guid.Empty)
        {
            return Result.Failure<Employee>(new DomainError("employee.organization_required", "Organization is required."));
        }

        var emailResult = EmailAddress.Create(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<Employee>(emailResult.Error);
        }

        try
        {
            var name = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName), 200);
            return Result.Success(new Employee(Guid.NewGuid(), organizationId, name, emailResult.Value));
        }
        catch (ArgumentException exception)
        {
            return Result.Failure<Employee>(new DomainError("employee.invalid", exception.Message));
        }
    }
}

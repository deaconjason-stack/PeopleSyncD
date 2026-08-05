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
            return Result<Employee>.Failure(new Error("employee.organization_required", "Organization is required."));
        }

        var emailResult = EmailAddress.Create(email);
        if (emailResult.IsFailure)
        {
            return Result<Employee>.Failure(emailResult.Error);
        }

        try
        {
            var name = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName), 200);
            return Result<Employee>.Success(new Employee(Guid.NewGuid(), organizationId, name, emailResult.Value));
        }
        catch (ArgumentException exception)
        {
            return Result<Employee>.Failure(new Error("employee.invalid", exception.Message));
        }
    }
}

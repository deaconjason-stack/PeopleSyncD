using FluentValidation;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Employees;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Employees;

public sealed class EmployeeService(
    IValidator<CreateEmployeeRequest> createValidator,
    IValidator<UpdateEmployeeRequest> updateValidator,
    IValidator<ChangeEmploymentStatusRequest> statusValidator,
    IEmployeeRepository employees,
    IAuditRecorder audit,
    IClock clock)
{
    public async Task<Result<EmployeeDto>> CreateAsync(
        Guid actorUserId,
        Guid organizationId,
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<EmployeeDto>(validation.Errors.Select(error => error.ErrorMessage));
        }

        var managerCheck = await ValidateManagerAsync(
            organizationId,
            request.ManagerEmployeeId,
            cancellationToken);
        if (managerCheck.IsFailure)
        {
            return Result.Failure<EmployeeDto>(managerCheck.Error);
        }

        var creation = Employee.Create(
            organizationId,
            request.EmployeeNumber,
            request.DisplayName,
            request.Email,
            request.Title,
            request.Department,
            request.ManagerEmployeeId,
            request.Location,
            request.EmploymentType,
            request.StartDate);
        if (creation.IsFailure)
        {
            return Result.Failure<EmployeeDto>(creation.Error);
        }

        await employees.AddAsync(creation.Value, cancellationToken);
        await employees.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(new SecurityAuditEvent(
            "employee.created",
            actorUserId,
            organizationId,
            "employee",
            creation.Value.Id.ToString("D"),
            clock.UtcNow,
            new Dictionary<string, string>
            {
                ["employee_number"] = creation.Value.EmployeeNumber,
                ["status"] = creation.Value.Status.ToString(),
            }), cancellationToken);

        return Result.Success(ToDto(creation.Value));
    }

    public async Task<EmployeeDto?> GetAsync(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await employees.GetAsync(organizationId, employeeId, cancellationToken);
        return employee is null ? null : ToDto(employee);
    }

    public async Task<IReadOnlyCollection<EmployeeDto>> ListAsync(
        Guid organizationId,
        string? search,
        EmploymentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var items = await employees.ListAsync(organizationId, search, status, cancellationToken);
        return items.Select(ToDto).ToArray();
    }

    public async Task<Result<EmployeeDto>> UpdateAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid employeeId,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<EmployeeDto>(validation.Errors.Select(error => error.ErrorMessage));
        }

        var employee = await employees.GetAsync(organizationId, employeeId, cancellationToken);
        if (employee is null)
        {
            return NotFound();
        }

        var managerCheck = await ValidateManagerAsync(
            organizationId,
            request.ManagerEmployeeId,
            cancellationToken);
        if (managerCheck.IsFailure)
        {
            return Result.Failure<EmployeeDto>(managerCheck.Error);
        }

        var update = employee.UpdateProfile(
            request.DisplayName,
            request.Email,
            request.Title,
            request.Department,
            request.ManagerEmployeeId,
            request.Location,
            request.EmploymentType);
        if (update.IsFailure)
        {
            return Result.Failure<EmployeeDto>(update.Error);
        }

        await employees.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(new SecurityAuditEvent(
            "employee.profile.updated",
            actorUserId,
            organizationId,
            "employee",
            employee.Id.ToString("D"),
            clock.UtcNow), cancellationToken);
        return Result.Success(ToDto(employee));
    }

    public async Task<Result<EmployeeDto>> ChangeStatusAsync(
        Guid actorUserId,
        Guid organizationId,
        Guid employeeId,
        ChangeEmploymentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await statusValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationFailure<EmployeeDto>(validation.Errors.Select(error => error.ErrorMessage));
        }

        var employee = await employees.GetAsync(organizationId, employeeId, cancellationToken);
        if (employee is null)
        {
            return NotFound();
        }

        var previous = employee.Status;
        var transition = request.Status switch
        {
            EmploymentStatus.Active when employee.Status == EmploymentStatus.Onboarding => employee.Activate(),
            EmploymentStatus.Active when employee.Status == EmploymentStatus.Leave => employee.ReturnFromLeave(),
            EmploymentStatus.Leave => employee.PlaceOnLeave(),
            EmploymentStatus.Suspended => employee.Suspend(),
            EmploymentStatus.Separated => employee.Separate(request.SeparationDate!.Value),
            EmploymentStatus.Archived => employee.Archive(),
            _ => Result.Failure(new DomainError(
                "employee.invalid_transition",
                $"Employee status cannot change from {employee.Status} to {request.Status}.")),
        };
        if (transition.IsFailure)
        {
            return Result.Failure<EmployeeDto>(transition.Error);
        }

        await employees.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(new SecurityAuditEvent(
            "employee.status.changed",
            actorUserId,
            organizationId,
            "employee",
            employee.Id.ToString("D"),
            clock.UtcNow,
            new Dictionary<string, string>
            {
                ["from"] = previous.ToString(),
                ["to"] = employee.Status.ToString(),
            }), cancellationToken);
        return Result.Success(ToDto(employee));
    }

    private async Task<Result> ValidateManagerAsync(
        Guid organizationId,
        Guid? managerEmployeeId,
        CancellationToken cancellationToken)
    {
        if (managerEmployeeId is null)
        {
            return Result.Success();
        }

        var manager = await employees.GetAsync(organizationId, managerEmployeeId.Value, cancellationToken);
        return manager is null
            ? Result.Failure(new DomainError(
                "employee.manager_not_found",
                "The selected manager is not available in this organization."))
            : Result.Success();
    }

    private static Result<T> ValidationFailure<T>(IEnumerable<string> messages) =>
        Result.Failure<T>(new DomainError(
            "employee.validation_failed",
            string.Join(" ", messages)));

    private static Result<EmployeeDto> NotFound() =>
        Result.Failure<EmployeeDto>(new DomainError(
            "employee.not_found",
            "The employee was not found."));

    private static EmployeeDto ToDto(Employee employee) => new(
        employee.Id,
        employee.OrganizationId,
        employee.EmployeeNumber,
        employee.DisplayName,
        employee.Email.Value,
        employee.Title,
        employee.Department,
        employee.ManagerEmployeeId,
        employee.Location,
        employee.EmploymentType,
        employee.Status,
        employee.StartDate,
        employee.SeparationDate);
}

using PeopleSyncD.Domain.Employees;

namespace PeopleSyncD.Application.Employees;

public sealed record CreateEmployeeRequest(
    string EmployeeNumber,
    string DisplayName,
    string Email,
    string Title,
    string Department,
    Guid? ManagerEmployeeId,
    string Location,
    EmploymentType EmploymentType,
    DateOnly StartDate);

public sealed record UpdateEmployeeRequest(
    string DisplayName,
    string Email,
    string Title,
    string Department,
    Guid? ManagerEmployeeId,
    string Location,
    EmploymentType EmploymentType);

public sealed record ChangeEmploymentStatusRequest(
    EmploymentStatus Status,
    DateOnly? SeparationDate = null);

public sealed record EmployeeDto(
    Guid Id,
    Guid OrganizationId,
    string EmployeeNumber,
    string DisplayName,
    string Email,
    string Title,
    string Department,
    Guid? ManagerEmployeeId,
    string Location,
    EmploymentType EmploymentType,
    EmploymentStatus Status,
    DateOnly StartDate,
    DateOnly? SeparationDate);

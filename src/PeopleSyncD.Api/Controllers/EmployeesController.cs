using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Employees;
using PeopleSyncD.Domain.Employees;
using PeopleSyncD.Domain.Permissions;

namespace PeopleSyncD.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
public sealed class EmployeesController(EmployeeService employees) : ControllerBase
{
    [Authorize(Policy = PermissionNames.EmployeesRead)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeDto>>> List(
        [FromQuery] string? search,
        [FromQuery] EmploymentStatus? status,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId))
        {
            return Forbid();
        }

        return Ok(await employees.ListAsync(tenantId, search, status, cancellationToken));
    }

    [Authorize(Policy = PermissionNames.EmployeesWrite)]
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId) || !User.TryGetUserId(out var userId))
        {
            return Forbid();
        }

        var result = await employees.CreateAsync(userId, tenantId, request, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ToProblem(result.Error.Code, result.Error.Description);
    }

    [Authorize(Policy = PermissionNames.EmployeesRead)]
    [HttpGet("{employeeId:guid}")]
    public async Task<ActionResult<EmployeeDto>> Get(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId))
        {
            return Forbid();
        }

        var employee = await employees.GetAsync(tenantId, employeeId, cancellationToken);
        return employee is null ? NotFound() : Ok(employee);
    }

    [Authorize(Policy = PermissionNames.EmployeesWrite)]
    [HttpPut("{employeeId:guid}")]
    public async Task<ActionResult<EmployeeDto>> Update(
        Guid employeeId,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId) || !User.TryGetUserId(out var userId))
        {
            return Forbid();
        }

        var result = await employees.UpdateAsync(userId, tenantId, employeeId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error.Code, result.Error.Description);
    }

    [Authorize(Policy = PermissionNames.EmployeesWrite)]
    [HttpPost("{employeeId:guid}/status")]
    public async Task<ActionResult<EmployeeDto>> ChangeStatus(
        Guid employeeId,
        ChangeEmploymentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId) || !User.TryGetUserId(out var userId))
        {
            return Forbid();
        }

        var result = await employees.ChangeStatusAsync(userId, tenantId, employeeId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error.Code, result.Error.Description);
    }

    private ObjectResult ToProblem(string code, string detail)
    {
        var status = code == "employee.not_found"
            ? StatusCodes.Status404NotFound
            : code.Contains("conflict", StringComparison.Ordinal)
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: code, detail: detail);
    }
}

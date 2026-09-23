using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Onboarding;
using PeopleSyncD.Domain.Permissions;

namespace PeopleSyncD.Api.Controllers;

[ApiController]
[Route("api/v1/employees/{employeeId:guid}/onboarding")]
public sealed class OnboardingController(OnboardingService onboarding) : ControllerBase
{
    [Authorize(Policy = PermissionNames.OnboardingRead)]
    [HttpGet]
    public async Task<ActionResult<EmployeeOnboardingDto>> Get(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId))
        {
            return Forbid();
        }

        var result = await onboarding.GetOrCreateAsync(tenantId, employeeId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error.Code, result.Error.Description);
    }

    [Authorize(Policy = PermissionNames.OnboardingWrite)]
    [HttpPut("tasks/{taskId:guid}")]
    public async Task<ActionResult<EmployeeOnboardingDto>> UpdateTask(
        Guid employeeId,
        Guid taskId,
        UpdateOnboardingTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId) || !User.TryGetUserId(out var userId))
        {
            return Forbid();
        }

        var result = await onboarding.UpdateTaskAsync(
            userId,
            tenantId,
            employeeId,
            taskId,
            request,
            cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblem(result.Error.Code, result.Error.Description);
    }

    private ObjectResult ToProblem(string code, string detail)
    {
        var status = code.Contains("not_found", StringComparison.Ordinal)
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: code, detail: detail);
    }
}

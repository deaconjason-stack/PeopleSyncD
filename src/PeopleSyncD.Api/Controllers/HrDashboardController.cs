using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Hr;
using PeopleSyncD.Domain.Permissions;

namespace PeopleSyncD.Api.Controllers;

[ApiController]
[Route("api/v1/hr/dashboard")]
public sealed class HrDashboardController(HrDashboardService dashboard) : ControllerBase
{
    [Authorize(Policy = PermissionNames.EmployeesRead)]
    [HttpGet]
    public async Task<ActionResult<HrDashboardDto>> Get(CancellationToken cancellationToken)
    {
        if (!User.TryGetTenantId(out var tenantId))
        {
            return Forbid();
        }

        return Ok(await dashboard.GetAsync(tenantId, cancellationToken));
    }
}

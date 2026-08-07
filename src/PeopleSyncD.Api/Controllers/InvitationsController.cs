using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Application.Identity;

namespace PeopleSyncD.Api.Controllers;

[ApiController]
[Route("api/v1/invitations")]
public sealed class InvitationsController(AcceptInvitationService acceptance) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("accept")]
    public async Task<ActionResult<OrganizationAccessDto>> Accept(
        AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await acceptance.ExecuteAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(
                statusCode: result.Error.Code.Contains("conflict", StringComparison.Ordinal)
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status400BadRequest,
                title: result.Error.Code,
                detail: result.Error.Description);
    }
}

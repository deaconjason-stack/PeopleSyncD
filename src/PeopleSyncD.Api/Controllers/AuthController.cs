using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Api.Controllers;

/// <summary>
/// Authentication and tenant-context endpoints.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    RegisterTenantService registration,
    LoginService login,
    ListOrganizationsService organizations,
    SelectOrganizationService selection,
    IIdentityGateway identities) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register-tenant")]
    [ProducesResponseType<AccessTokenDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccessTokenDto>> RegisterTenant(
        RegisterTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await registration.ExecuteAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        var status = result.Error.Code.Contains("conflict", StringComparison.Ordinal)
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: result.Error.Code, detail: result.Error.Description);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AccessTokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccessTokenDto>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await login.ExecuteAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var status = result.Error.Code == "authentication.invalid_credentials"
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpGet("organizations")]
    [ProducesResponseType<IReadOnlyCollection<OrganizationAccessDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<OrganizationAccessDto>>> ListOrganizations(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await organizations.ExecuteAsync(userId, cancellationToken));
    }

    [Authorize]
    [HttpPost("select-organization")]
    [ProducesResponseType<AccessTokenDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AccessTokenDto>> SelectOrganization(
        SelectOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await selection.ExecuteAsync(userId, request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(
                statusCode: result.Error.Code == "tenant.access_denied"
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status400BadRequest,
                title: result.Error.Code,
                detail: result.Error.Description);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<CurrentSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentSessionDto>> GetCurrentSession(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await identities.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? Unauthorized()
            : Ok(new CurrentSessionDto(user, User.GetTenantContext()));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Identity;

namespace PeopleSyncD.Api.Controllers;

[ApiController]
[Route("api/v1/auth/passkeys")]
public sealed class PasskeysController(
    PasskeySecurityService passkeys,
    PrivilegedAuthenticationPolicy privilegedAuthentication) : ControllerBase
{
    [Authorize]
    [HttpPost("registration/options")]
    public async Task<ActionResult<PasskeyCeremonyOptionsDto>> BeginRegistration(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var freshness = privilegedAuthentication.Validate(User.GetAuthenticationTime());
        if (freshness.IsFailure)
        {
            return ReauthenticationRequired(freshness.Error);
        }

        var result = await passkeys.BeginRegistrationAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpPost("registration/complete")]
    public async Task<ActionResult<PasskeyCredentialDto>> CompleteRegistration(
        CompletePasskeyRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var freshness = privilegedAuthentication.Validate(User.GetAuthenticationTime());
        if (freshness.IsFailure)
        {
            return ReauthenticationRequired(freshness.Error);
        }

        var result = await passkeys.CompleteRegistrationAsync(userId, request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [AllowAnonymous]
    [HttpPost("authentication/options")]
    public async Task<ActionResult<PasskeyCeremonyOptionsDto>> BeginAuthentication(
        BeginPasskeyAuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await passkeys.BeginLoginAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status401Unauthorized, title: result.Error.Code, detail: result.Error.Description);
    }

    [AllowAnonymous]
    [HttpPost("authentication/complete")]
    public async Task<ActionResult<AccessTokenDto>> CompleteAuthentication(
        CompletePasskeyAuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await passkeys.CompleteLoginAsync(
            request,
            Request.Headers.UserAgent.ToString(),
            cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status401Unauthorized, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpPost("step-up/options")]
    public async Task<ActionResult<PasskeyCeremonyOptionsDto>> BeginStepUp(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await passkeys.BeginStepUpAsync(userId, User.GetTenantContext(), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpPost("step-up/complete")]
    public async Task<ActionResult<AccessTokenDto>> CompleteStepUp(
        CompletePasskeyAuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await passkeys.CompleteStepUpAsync(
            userId,
            request,
            Request.Headers.UserAgent.ToString(),
            cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status401Unauthorized, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PasskeyCredentialDto>>> List(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await passkeys.ListAsync(userId, cancellationToken));
    }

    [Authorize]
    [HttpDelete("{credentialId:guid}")]
    public async Task<IActionResult> Revoke(Guid credentialId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var freshness = privilegedAuthentication.Validate(User.GetAuthenticationTime());
        if (freshness.IsFailure)
        {
            return ReauthenticationRequired(freshness.Error);
        }

        var result = await passkeys.RevokeAsync(userId, credentialId, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error.Code, detail: result.Error.Description);
    }

    private ObjectResult ReauthenticationRequired(PeopleSyncD.SharedKernel.DomainError error) =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: error.Code,
            detail: error.Description);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;

namespace PeopleSyncD.Api.Controllers;

/// <summary>
/// Authentication, account security, and tenant-context endpoints.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    RegisterTenantService registration,
    LoginService login,
    ListOrganizationsService organizations,
    SelectOrganizationService selection,
    RequestEmailVerificationService emailVerificationRequest,
    ConfirmEmailService emailConfirmation,
    RefreshSessionService refresh,
    MfaSecurityService mfa,
    SessionAdministrationService sessionAdministration,
    IIdentityGateway identities) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register-tenant")]
    public async Task<ActionResult<AccessTokenDto>> RegisterTenant(
        RegisterTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await registration.ExecuteAsync(request, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : Problem(
                statusCode: result.Error.Code.Contains("conflict", StringComparison.Ordinal)
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status400BadRequest,
                title: result.Error.Code,
                detail: result.Error.Description);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await login.ExecuteAsync(
            request,
            cancellationToken,
            Request.Headers.UserAgent.ToString());
        if (result.IsFailure)
        {
            var status = result.Error.Code == "authentication.invalid_credentials"
                ? StatusCodes.Status401Unauthorized
                : StatusCodes.Status400BadRequest;
            return Problem(statusCode: status, title: result.Error.Code, detail: result.Error.Description);
        }

        if (result.Value.Challenge is not null)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, result.Value.Challenge);
        }

        return Ok(result.Value.Session);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokenDto>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await refresh.ExecuteAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: result.Error.Code,
                detail: result.Error.Description);
    }

    [AllowAnonymous]
    [HttpPost("mfa/complete")]
    public async Task<ActionResult<AccessTokenDto>> CompleteMfa(
        MfaChallengeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mfa.CompleteChallengeAsync(
            request,
            cancellationToken,
            Request.Headers.UserAgent.ToString());
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: result.Error.Code,
                detail: result.Error.Description);
    }

    [Authorize]
    [HttpPost("mfa/totp/enroll")]
    public async Task<ActionResult<MfaTotpEnrollmentDto>> BeginTotpEnrollment(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await mfa.BeginTotpEnrollmentAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpPost("mfa/totp/confirm")]
    public async Task<ActionResult<RecoveryCodeBatchDto>> ConfirmTotpEnrollment(
        ConfirmTotpEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await mfa.ConfirmTotpEnrollmentAsync(userId, request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpPost("mfa/step-up")]
    public async Task<ActionResult<MfaChallengeDto>> StartStepUp(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await mfa.StartStepUpAsync(userId, User.GetTenantContext(), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpPost("mfa/recovery-codes/regenerate")]
    public async Task<ActionResult<RecoveryCodeBatchDto>> RegenerateRecoveryCodes(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId)
            || !string.Equals(User.GetAssuranceLevel(), "mfa", StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        var result = await mfa.RegenerateRecoveryCodesAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyCollection<SessionSummaryDto>>> ListSessions(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var currentFamily = User.TryGetSessionFamilyId(out var familyId) ? familyId : (Guid?)null;
        return Ok(await sessionAdministration.ListAsync(userId, currentFamily, cancellationToken));
    }

    [Authorize]
    [HttpDelete("sessions/{familyId:guid}")]
    public async Task<IActionResult> RevokeSession(
        Guid familyId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await sessionAdministration.RevokeAsync(userId, familyId, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpPost("sessions/revoke-others")]
    public async Task<IActionResult> RevokeOtherSessions(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId) || !User.TryGetSessionFamilyId(out var familyId))
        {
            return Unauthorized();
        }

        await sessionAdministration.RevokeOthersAsync(userId, familyId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("security-events")]
    public async Task<ActionResult<IReadOnlyCollection<SecurityEventDto>>> ListSecurityEvents(
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await mfa.ListSecurityEventsAsync(userId, cancellationToken));
    }

    [Authorize]
    [HttpPost("email-verification/request")]
    public async Task<IActionResult> RequestEmailVerification(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await emailVerificationRequest.ExecuteAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Accepted()
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [AllowAnonymous]
    [HttpPost("email-verification/confirm")]
    public async Task<IActionResult> ConfirmEmail(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await emailConfirmation.ExecuteAsync(request, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpGet("organizations")]
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
    public async Task<ActionResult<AccessTokenDto>> SelectOrganization(
        SelectOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await selection.ExecuteAsync(
            userId,
            request,
            cancellationToken,
            User.GetAssuranceLevel(),
            Request.Headers.UserAgent.ToString());
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var status = result.Error.Code is "tenant.access_denied"
            or "authentication.email_verification_required"
            or "authentication.mfa_required"
            ? StatusCodes.Status403Forbidden
            : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: result.Error.Code, detail: result.Error.Description);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentSessionDto>> GetCurrentSession(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await identities.GetByIdAsync(userId, cancellationToken);
        return user is null ? Unauthorized() : Ok(new CurrentSessionDto(user, User.GetTenantContext()));
    }

    [Authorize]
    [HttpGet("security")]
    public async Task<ActionResult<AccountSecurityDto>> GetSecurity(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await identities.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        var recoveryCodes = await mfa.GetRecoveryCodeCountAsync(userId, cancellationToken);
        return Ok(new AccountSecurityDto(
            user.Id,
            user.EmailConfirmed,
            user.MfaEnabled,
            !user.MfaEnabled,
            recoveryCodes));
    }
}

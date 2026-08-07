using FluentValidation;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Authenticates a platform identity before tenant selection.
/// </summary>
public sealed class LoginService(
    IValidator<LoginRequest> validator,
    IIdentityGateway identities,
    IMfaSecurityGateway mfa,
    SessionTokenService sessions)
{
    public async Task<Result<LoginOutcomeDto>> ExecuteAsync(
        LoginRequest request,
        string? deviceLabel = null,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<LoginOutcomeDto>(new DomainError(
                "authentication.validation_failed",
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        var identity = await identities.ValidateCredentialsAsync(
            request.Email.Trim().ToLowerInvariant(),
            request.Password,
            cancellationToken);
        if (identity.IsFailure)
        {
            return Result.Failure<LoginOutcomeDto>(identity.Error);
        }

        if (identity.Value.MfaEnabled)
        {
            var challenge = await mfa.CreateChallengeAsync(
                identity.Value.Id,
                "login",
                cancellationToken: cancellationToken);
            return challenge.IsFailure
                ? Result.Failure<LoginOutcomeDto>(challenge.Error)
                : Result.Success(new LoginOutcomeDto(null, challenge.Value));
        }

        var session = await sessions.IssueAsync(
            identity.Value,
            assuranceLevel: "pwd",
            deviceLabel: deviceLabel,
            cancellationToken: cancellationToken);
        return Result.Success(new LoginOutcomeDto(session, null));
    }
}

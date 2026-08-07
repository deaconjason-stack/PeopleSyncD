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
    SessionTokenService sessions)
{
    public async Task<Result<AccessTokenDto>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.Failure<AccessTokenDto>(new DomainError(
                "authentication.validation_failed",
                string.Join(" ", validation.Errors.Select(error => error.ErrorMessage))));
        }

        var identity = await identities.ValidateCredentialsAsync(
            request.Email.Trim().ToLowerInvariant(),
            request.Password,
            cancellationToken);
        if (identity.IsFailure)
        {
            return Result.Failure<AccessTokenDto>(identity.Error);
        }

        return Result.Success(await sessions.IssueAsync(identity.Value, cancellationToken: cancellationToken));
    }
}

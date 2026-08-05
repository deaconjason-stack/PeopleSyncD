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
    IAccessTokenIssuer tokenIssuer)
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
        return identity.IsFailure
            ? Result.Failure<AccessTokenDto>(identity.Error)
            : Result.Success(tokenIssuer.Issue(identity.Value));
    }
}

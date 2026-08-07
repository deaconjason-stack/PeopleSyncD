using Microsoft.AspNetCore.Identity;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class IdentityGateway(UserManager<ApplicationUser> users) : IIdentityGateway
{
    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await users.FindByEmailAsync(email) is not null;
    }

    public async Task<Result<IdentityUserDto>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByEmailAsync(email);
        if (user is null || !user.IsActive || await users.IsLockedOutAsync(user))
        {
            return InvalidCredentials();
        }

        if (!await users.CheckPasswordAsync(user, password))
        {
            await users.AccessFailedAsync(user);
            return InvalidCredentials();
        }

        await users.ResetAccessFailedCountAsync(user);
        return Result.Success(ToDto(user));
    }

    public async Task<IdentityUserDto?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        return user is null ? null : ToDto(user);
    }

    private static Result<IdentityUserDto> InvalidCredentials() =>
        Result.Failure<IdentityUserDto>(new DomainError(
            "authentication.invalid_credentials",
            "The email address or password is invalid."));

    private static IdentityUserDto ToDto(ApplicationUser user) => new(
        user.Id,
        user.DisplayName,
        user.Email ?? string.Empty,
        user.EmailConfirmed,
        user.IsActive,
        user.TwoFactorEnabled);
}

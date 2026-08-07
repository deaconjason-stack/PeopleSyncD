using Microsoft.AspNetCore.Identity;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure.Identity;

internal sealed class IdentityAdministrationGateway(UserManager<ApplicationUser> users)
    : IIdentityAdministrationGateway
{
    public async Task<IdentityAdministrationUserDto?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByEmailAsync(email.Trim().ToLowerInvariant());
        return user is null ? null : ToDto(user);
    }

    public async Task<Result<IdentityAdministrationUserDto>> CreateInvitedUserAsync(
        string email,
        string displayName,
        string password,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = displayName.Trim(),
            EmailConfirmed = false,
            IsActive = true,
        };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result.Failure<IdentityAdministrationUserDto>(new DomainError(
                "identity.create_failed",
                string.Join(" ", result.Errors.Select(error => error.Description))));
        }

        return Result.Success(ToDto(user));
    }

    public async Task<Result> ConfirmEmailFromInvitationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        if (user is null)
        {
            return Result.Failure(new DomainError("identity.user_missing", "The user is unavailable."));
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        user.EmailConfirmed = true;
        var result = await users.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(new DomainError(
                "identity.email_confirmation_failed",
                string.Join(" ", result.Errors.Select(error => error.Description))));
    }

    private static IdentityAdministrationUserDto ToDto(ApplicationUser user) => new(
        user.Id,
        user.DisplayName,
        user.Email ?? string.Empty,
        user.EmailConfirmed,
        user.IsActive,
        user.TwoFactorEnabled);
}

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
        return ToResult(result, "identity.email_confirmation_failed");
    }

    public async Task<Result<string>> GenerateEmailVerificationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        if (user is null || !user.IsActive)
        {
            return Result.Failure<string>(new DomainError("identity.user_missing", "The user is unavailable."));
        }

        return Result.Success(await users.GenerateEmailConfirmationTokenAsync(user));
    }

    public async Task<Result> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await users.FindByIdAsync(userId.ToString("D"));
        if (user is null || !user.IsActive)
        {
            return Result.Failure(new DomainError("identity.user_missing", "The user is unavailable."));
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        var result = await users.ConfirmEmailAsync(user, token);
        return ToResult(result, "identity.email_verification_failed");
    }

    private static Result ToResult(IdentityResult result, string code) =>
        result.Succeeded
            ? Result.Success()
            : Result.Failure(new DomainError(
                code,
                string.Join(" ", result.Errors.Select(error => error.Description))));

    private static IdentityAdministrationUserDto ToDto(ApplicationUser user) => new(
        user.Id,
        user.DisplayName,
        user.Email ?? string.Empty,
        user.EmailConfirmed,
        user.IsActive,
        user.TwoFactorEnabled);
}

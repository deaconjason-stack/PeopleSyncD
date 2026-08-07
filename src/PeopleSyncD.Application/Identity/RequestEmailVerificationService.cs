using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class RequestEmailVerificationService(
    IIdentityGateway identities,
    IIdentityAdministrationGateway administration,
    IIdentityNotificationSender notifications,
    IAuditRecorder audit,
    IClock clock)
{
    public async Task<Result> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await identities.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result.Failure(new DomainError("identity.user_missing", "The user is unavailable."));
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        var token = await administration.GenerateEmailVerificationTokenAsync(userId, cancellationToken);
        if (token.IsFailure)
        {
            return Result.Failure(token.Error);
        }

        await notifications.SendEmailVerificationAsync(user.Email, user.Id, token.Value, cancellationToken);
        await audit.RecordAsync(new SecurityAuditEvent(
            "identity.email_verification.requested",
            user.Id,
            null,
            "user",
            user.Id.ToString("D"),
            clock.UtcNow), cancellationToken);
        return Result.Success();
    }
}

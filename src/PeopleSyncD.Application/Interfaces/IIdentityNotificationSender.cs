using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Application.Interfaces;

public interface IIdentityNotificationSender
{
    Task SendInvitationAsync(
        string email,
        string organizationName,
        TenantRole role,
        string acceptanceToken,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default);

    Task SendEmailVerificationAsync(
        string email,
        Guid userId,
        string verificationToken,
        CancellationToken cancellationToken = default);
}

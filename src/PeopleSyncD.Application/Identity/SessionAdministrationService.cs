using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Provides user-scoped refresh-session inventory and revocation operations.
/// </summary>
public sealed class SessionAdministrationService(IRefreshSessionGateway sessions)
{
    public Task<IReadOnlyCollection<SessionSummaryDto>> ListAsync(
        Guid userId,
        Guid? currentFamilyId,
        CancellationToken cancellationToken = default) =>
        sessions.ListForUserAsync(userId, currentFamilyId, cancellationToken);

    public Task<Result> RevokeAsync(
        Guid userId,
        Guid familyId,
        CancellationToken cancellationToken = default) =>
        sessions.RevokeUserFamilyAsync(userId, familyId, "user_revoked", cancellationToken);

    public Task RevokeOthersAsync(
        Guid userId,
        Guid currentFamilyId,
        CancellationToken cancellationToken = default) =>
        sessions.RevokeOtherFamiliesAsync(userId, currentFamilyId, "user_revoked_other_sessions", cancellationToken);
}

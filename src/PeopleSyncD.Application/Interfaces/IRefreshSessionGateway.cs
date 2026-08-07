using PeopleSyncD.Application.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Interfaces;

public interface IRefreshSessionGateway
{
    Task<RefreshTokenDto> IssueAsync(
        Guid userId,
        Guid? organizationId,
        Guid? membershipId,
        Guid? familyId = null,
        string assuranceLevel = "pwd",
        string? deviceLabel = null,
        DateTimeOffset? authenticatedAt = null,
        string? authenticationMethod = null,
        CancellationToken cancellationToken = default);

    Task<Result<RefreshRotationDto>> RotateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeFamilyAsync(
        Guid familyId,
        string reason,
        CancellationToken cancellationToken = default);

    Task RevokeForMembershipAsync(
        Guid membershipId,
        string reason,
        CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeUserFamilyAsync(
        Guid userId,
        Guid familyId,
        string reason,
        CancellationToken cancellationToken = default);

    Task RevokeOtherFamiliesAsync(
        Guid userId,
        Guid currentFamilyId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<bool> IsFamilyActiveAsync(
        Guid userId,
        Guid familyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SessionSummaryDto>> ListForUserAsync(
        Guid userId,
        Guid? currentFamilyId,
        CancellationToken cancellationToken = default);
}

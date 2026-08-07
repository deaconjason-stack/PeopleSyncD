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
}

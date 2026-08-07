using PeopleSyncD.Application.Identity;
using PeopleSyncD.Domain.Identity;

namespace PeopleSyncD.Application.Interfaces;

public interface IOrganizationInvitationRepository
{
    Task AddAsync(OrganizationInvitation invitation, CancellationToken cancellationToken = default);

    Task<OrganizationInvitation?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingAsync(
        Guid organizationId,
        string email,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InvitationDto>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}

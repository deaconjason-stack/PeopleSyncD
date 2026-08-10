using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Infrastructure.Repositories;

internal sealed class OrganizationInvitationRepository(ApplicationDbContext database)
    : IOrganizationInvitationRepository
{
    public async Task AddAsync(
        OrganizationInvitation invitation,
        CancellationToken cancellationToken = default) =>
        await database.OrganizationInvitations.AddAsync(invitation, cancellationToken);

    public Task<OrganizationInvitation?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        database.OrganizationInvitations.SingleOrDefaultAsync(
            invitation => invitation.TokenHash == tokenHash,
            cancellationToken);

    public Task<bool> HasPendingAsync(
        Guid organizationId,
        string email,
        CancellationToken cancellationToken = default) =>
        database.OrganizationInvitations.AnyAsync(
            invitation => invitation.OrganizationId == organizationId
                && invitation.Email == email
                && invitation.Status == InvitationStatus.Pending,
            cancellationToken);

    public async Task<IReadOnlyCollection<InvitationDto>> ListAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var invitations = await database.OrganizationInvitations
            .AsNoTracking()
            .Where(invitation => invitation.OrganizationId == organizationId)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .Select(invitation => new InvitationDto(
                invitation.Id,
                invitation.OrganizationId,
                invitation.Email,
                invitation.DisplayName,
                invitation.Role,
                invitation.Status,
                invitation.CreatedAt,
                invitation.ExpiresAt))
            .ToListAsync(cancellationToken);
        return invitations.AsReadOnly();
    }
}

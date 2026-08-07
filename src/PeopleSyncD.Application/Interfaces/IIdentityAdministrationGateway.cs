using PeopleSyncD.Application.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Interfaces;

public interface IIdentityAdministrationGateway
{
    Task<IdentityAdministrationUserDto?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<Result<IdentityAdministrationUserDto>> CreateInvitedUserAsync(
        string email,
        string displayName,
        string password,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailFromInvitationAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

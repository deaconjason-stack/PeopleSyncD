using PeopleSyncD.Application.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Interfaces;

/// <summary>
/// Identity persistence boundary used by authentication use cases.
/// </summary>
public interface IIdentityGateway
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<Result<IdentityUserDto>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<IdentityUserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

using PeopleSyncD.Application.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Interfaces;

public interface IPasskeySecurityGateway
{
    Task<Result<PasskeyCeremonyOptionsDto>> BeginRegistrationAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<PasskeyCredentialDto>> CompleteRegistrationAsync(
        Guid userId,
        CompletePasskeyRegistrationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PasskeyCeremonyOptionsDto>> BeginAuthenticationAsync(
        Guid userId,
        string purpose,
        Guid? organizationId = null,
        Guid? membershipId = null,
        CancellationToken cancellationToken = default);

    Task<Result<PasskeyAuthenticationCompletionDto>> CompleteAuthenticationAsync(
        CompletePasskeyAuthenticationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PasskeyCredentialDto>> ListAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeAsync(
        Guid userId,
        Guid credentialId,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

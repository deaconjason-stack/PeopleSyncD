using PeopleSyncD.Application.Identity;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Interfaces;

/// <summary>
/// Persists and verifies multi-factor enrollment, challenges, and recovery factors.
/// </summary>
public interface IMfaSecurityGateway
{
    Task<Result<MfaTotpEnrollmentDto>> BeginTotpEnrollmentAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<RecoveryCodeBatchDto>> ConfirmTotpEnrollmentAsync(
        Guid userId,
        string code,
        CancellationToken cancellationToken = default);

    Task<Result<RecoveryCodeBatchDto>> RegenerateRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<MfaChallengeDto>> CreateChallengeAsync(
        Guid userId,
        string purpose,
        Guid? organizationId = null,
        Guid? membershipId = null,
        CancellationToken cancellationToken = default);

    Task<Result<MfaChallengeCompletionDto>> CompleteChallengeAsync(
        MfaChallengeRequest request,
        CancellationToken cancellationToken = default);

    Task<int> GetRecoveryCodeCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SecurityEventDto>> ListSecurityEventsAsync(
        Guid userId,
        int limit = 25,
        CancellationToken cancellationToken = default);
}

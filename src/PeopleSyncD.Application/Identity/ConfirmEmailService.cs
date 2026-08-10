using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class ConfirmEmailService(
    IIdentityAdministrationGateway administration,
    IAuditRecorder audit,
    IClock clock)
{
    public async Task<Result> ExecuteAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Token))
        {
            return Result.Failure(new DomainError(
                "identity.email_verification_invalid",
                "A valid user and verification token are required."));
        }

        var result = await administration.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
        if (result.IsFailure)
        {
            return result;
        }

        await audit.RecordAsync(new SecurityAuditEvent(
            "identity.email_verified",
            request.UserId,
            null,
            "user",
            request.UserId.ToString("D"),
            clock.UtcNow), cancellationToken);
        return Result.Success();
    }
}

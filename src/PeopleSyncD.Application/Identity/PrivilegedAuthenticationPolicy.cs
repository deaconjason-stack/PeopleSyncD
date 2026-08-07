using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Application.Identity;

public sealed class PrivilegedAuthenticationPolicy(IClock clock)
{
    public static readonly TimeSpan MaximumAge = TimeSpan.FromMinutes(5);

    public Result Validate(DateTimeOffset? authenticatedAt)
    {
        if (authenticatedAt is null
            || authenticatedAt > clock.UtcNow.AddMinutes(1)
            || clock.UtcNow - authenticatedAt > MaximumAge)
        {
            return Result.Failure(new DomainError(
                "authentication.reauthentication_required",
                "Recent authentication is required for this sensitive operation."));
        }

        return Result.Success();
    }
}

namespace PeopleSyncD.Application.Identity;

public sealed record CurrentUser(Guid UserId, string Email, string DisplayName);

public interface ICurrentUserAccessor
{
    CurrentUser? GetCurrentUser();
}

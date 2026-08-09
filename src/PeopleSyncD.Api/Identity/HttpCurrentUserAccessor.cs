using System.Security.Claims;
using PeopleSyncD.Application.Identity;

namespace PeopleSyncD.Api.Identity;

public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public CurrentUser? GetCurrentUser()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true) return null;

        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
        var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue("name");

        return Guid.TryParse(idValue, out var id) && !string.IsNullOrWhiteSpace(email)
            ? new CurrentUser(id, email, displayName ?? email)
            : null;
    }
}

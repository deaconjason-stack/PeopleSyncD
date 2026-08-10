using Microsoft.AspNetCore.Identity;

namespace PeopleSyncD.Infrastructure.Identity;

/// <summary>
/// Persistence identity record. Domain user behavior remains outside ASP.NET Core Identity.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid? PersonId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

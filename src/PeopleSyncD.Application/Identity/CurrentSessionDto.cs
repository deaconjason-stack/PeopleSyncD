namespace PeopleSyncD.Application.Identity;

/// <summary>
/// Current validated identity and optional tenant context.
/// </summary>
public sealed record CurrentSessionDto(
    IdentityUserDto User,
    TenantContextDto? Tenant);

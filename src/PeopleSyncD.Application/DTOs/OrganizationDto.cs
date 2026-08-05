namespace PeopleSyncD.Application.DTOs;

/// <summary>
/// Public organization representation.
/// </summary>
public sealed record OrganizationDto(Guid Id, string Name, string Slug, DateTimeOffset CreatedAt);

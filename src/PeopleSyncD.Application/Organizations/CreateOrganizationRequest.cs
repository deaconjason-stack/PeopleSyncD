namespace PeopleSyncD.Application.Organizations;

/// <summary>
/// Request to create an organization.
/// </summary>
public sealed record CreateOrganizationRequest(string Name, string Slug);

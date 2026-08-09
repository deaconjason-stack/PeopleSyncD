namespace PeopleSyncD.Domain.Organizations;

public sealed class Organization
{
    private Organization() { }

    public Organization(Guid id, string name, string slug)
    {
        if (id == Guid.Empty) throw new ArgumentException("Organization id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Organization name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Organization slug is required.", nameof(slug));

        Id = id;
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Status = OrganizationStatus.Active;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public OrganizationStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

public enum OrganizationStatus
{
    Active = 1,
    Suspended = 2,
    Archived = 3
}

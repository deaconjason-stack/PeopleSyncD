namespace PeopleSyncD.Domain.Identity;

public sealed class OrganizationMembership
{
    private OrganizationMembership() { }
    public OrganizationMembership(Guid id, Guid userId, Guid organizationId, MembershipRole role)
    {
        if (id == Guid.Empty || userId == Guid.Empty || organizationId == Guid.Empty) throw new ArgumentException("Membership identifiers are required.");
        Id = id; UserId = userId; OrganizationId = organizationId; Role = role; CreatedAtUtc = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public MembershipRole Role { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
public enum MembershipRole { Member = 1, Manager = 2, Administrator = 3, Owner = 4 }

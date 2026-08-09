namespace PeopleSyncD.Domain.People;

public sealed class Person
{
    private Person() { }

    public Person(Guid id, Guid organizationId, string firstName, string lastName, string email)
    {
        if (id == Guid.Empty) throw new ArgumentException("Person id is required.", nameof(id));
        if (organizationId == Guid.Empty) throw new ArgumentException("Organization id is required.", nameof(organizationId));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        Id = id;
        OrganizationId = organizationId;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Status = PersonStatus.Active;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public PersonStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

public enum PersonStatus
{
    Active = 1,
    Inactive = 2,
    Archived = 3
}

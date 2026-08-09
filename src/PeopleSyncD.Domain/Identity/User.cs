namespace PeopleSyncD.Domain.Identity;

public sealed class User
{
    private User() { }
    public User(Guid id, string email, string displayName)
    {
        if (id == Guid.Empty) throw new ArgumentException("User id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
        Id = id; Email = email.Trim().ToLowerInvariant(); DisplayName = displayName.Trim(); Status = UserStatus.Active; CreatedAtUtc = DateTimeOffset.UtcNow;
    }
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
public enum UserStatus { Active = 1, Suspended = 2, Disabled = 3 }

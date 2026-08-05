namespace PeopleSyncD.Domain.Identity;

/// <summary>
/// Strongly typed user identifier.
/// </summary>
/// <param name="Value">Underlying UUID.</param>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

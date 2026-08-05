namespace PeopleSyncD.SharedKernel;

/// <summary>
/// Describes a controlled application or domain failure.
/// </summary>
/// <param name="Code">Stable machine-readable error code.</param>
/// <param name="Description">Human-readable description.</param>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

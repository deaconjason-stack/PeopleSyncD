namespace PeopleSyncD.Domain.Common;

internal static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Value cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }
}

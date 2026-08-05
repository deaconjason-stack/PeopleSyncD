using System.Net.Mail;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.ValueObjects;

/// <summary>
/// Normalized email address value object.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    private EmailAddress(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<EmailAddress> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<EmailAddress>.Failure(new Error("email.required", "Email address is required."));
        }

        try
        {
            var parsed = new MailAddress(value.Trim());
            return Result<EmailAddress>.Success(new EmailAddress(parsed.Address.ToLowerInvariant()));
        }
        catch (FormatException)
        {
            return Result<EmailAddress>.Failure(new Error("email.invalid", "Email address format is invalid."));
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

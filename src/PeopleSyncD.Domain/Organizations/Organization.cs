using PeopleSyncD.Domain.Common;
using PeopleSyncD.Domain.Events;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Domain.Organizations;

/// <summary>
/// Tenant organization aggregate root.
/// </summary>
public sealed class Organization : AggregateRoot<Guid>
{
    private Organization()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    private Organization(Guid id, string name, string slug)
        : base(id)
    {
        Name = name;
        Slug = slug;
    }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<Organization> Create(string? name, string? slug, DateTimeOffset createdAt)
    {
        try
        {
            var normalizedName = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 200);
            var normalizedSlug = Guard.AgainstNullOrWhiteSpace(slug, nameof(slug), 80).ToLowerInvariant();
            var organization = new Organization(Guid.NewGuid(), normalizedName, normalizedSlug)
            {
                CreatedAt = createdAt,
            };
            organization.Raise(new OrganizationCreatedDomainEvent(organization.Id, organization.Name, createdAt));
            return Result.Success(organization);
        }
        catch (ArgumentException exception)
        {
            return Result.Failure<Organization>(new DomainError("organization.invalid", exception.Message));
        }
    }

    public Result Rename(string? name)
    {
        try
        {
            Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 200);
            return Result.Success();
        }
        catch (ArgumentException exception)
        {
            return Result.Failure(new DomainError("organization.invalid_name", exception.Message));
        }
    }
}

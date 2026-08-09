using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Domain.Organizations;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Api.Endpoints;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/organizations").WithTags("Organizations");

        group.MapGet("/", async (PeopleSyncDDbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await db.Organizations.AsNoTracking().OrderBy(x => x.Name).Select(x => new OrganizationResponse(x.Id, x.Name, x.Slug, x.Status.ToString(), x.CreatedAtUtc)).ToListAsync(cancellationToken)));

        group.MapPost("/", async (CreateOrganizationRequest request, PeopleSyncDDbContext db, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["organization"] = ["Name and slug are required."] });

            var exists = await db.Organizations.AnyAsync(x => x.Slug == request.Slug.Trim().ToLowerInvariant(), cancellationToken);
            if (exists) return Results.Conflict(new { message = "An organization with this slug already exists." });

            var organization = new Organization(Guid.NewGuid(), request.Name, request.Slug);
            db.Organizations.Add(organization);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/organizations/{organization.Id}", new OrganizationResponse(organization.Id, organization.Name, organization.Slug, organization.Status.ToString(), organization.CreatedAtUtc));
        });

        return endpoints;
    }

    public sealed record CreateOrganizationRequest(string Name, string Slug);
    public sealed record OrganizationResponse(Guid Id, string Name, string Slug, string Status, DateTimeOffset CreatedAtUtc);
}

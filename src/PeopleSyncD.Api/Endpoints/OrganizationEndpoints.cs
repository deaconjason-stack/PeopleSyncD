using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Infrastructure.Authorization;
using PeopleSyncD.Infrastructure.Persistence;
using PeopleSyncD.Domain.Identity;
using PeopleSyncD.Domain.Organizations;

namespace PeopleSyncD.Api.Endpoints;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/organizations").WithTags("Organizations");

        group.MapGet("/{organizationId:guid}", async (HttpContext http, Guid organizationId, PeopleSyncDDbContext db, TenantAccess tenant, CancellationToken cancellationToken) =>
        {
            if (!(await tenant.RequireMembershipAsync(http.User, organizationId, cancellationToken)).Allowed) return Results.Forbid();
            var organization = await db.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == organizationId, cancellationToken);
            return organization is null ? Results.NotFound() : Results.Ok(new OrganizationResponse(organization.Id, organization.Name, organization.Slug, organization.Status.ToString(), organization.CreatedAtUtc));
        });

        group.MapPost("/", async (HttpContext http, CreateOrganizationRequest request, PeopleSyncDDbContext db, CancellationToken cancellationToken) =>
        {
            if (http.User.Identity?.IsAuthenticated != true) return Results.Unauthorized();
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["organization"] = ["Name and slug are required."] });
            var slug = request.Slug.Trim().ToLowerInvariant();
            if (await db.Organizations.AnyAsync(x => x.Slug == slug, cancellationToken)) return Results.Conflict(new { message = "An organization with this slug already exists." });
            var organization = new Organization(Guid.NewGuid(), request.Name, slug);
            db.Organizations.Add(organization); await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/organizations/{organization.Id}", new OrganizationResponse(organization.Id, organization.Name, organization.Slug, organization.Status.ToString(), organization.CreatedAtUtc));
        });
        return endpoints;
    }
    public sealed record CreateOrganizationRequest(string Name, string Slug);
    public sealed record OrganizationResponse(Guid Id, string Name, string Slug, string Status, DateTimeOffset CreatedAtUtc);
}

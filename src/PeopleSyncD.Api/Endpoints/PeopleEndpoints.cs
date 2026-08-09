using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Infrastructure.Persistence;
using PeopleSyncD.Infrastructure.Authorization;

namespace PeopleSyncD.Api.Endpoints;

public static class PeopleEndpoints
{
    public static IEndpointRouteBuilder MapPeopleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/people").WithTags("People");

        group.MapGet("/", async (HttpContext http, Guid organizationId, PeopleSyncDDbContext db, TenantAccess tenant, CancellationToken cancellationToken) =>
        {
            if (!(await tenant.RequireMembershipAsync(http.User, organizationId, cancellationToken)).Allowed) return Results.Forbid();
            return Results.Ok(await db.People.AsNoTracking().Where(x => x.OrganizationId == organizationId).OrderBy(x => x.LastName).Select(x => new PersonResponse(x.Id, x.OrganizationId, x.FirstName, x.LastName, x.Email, x.Status.ToString())).ToListAsync(cancellationToken));
        });

        group.MapPost("/", async (HttpContext http, CreatePersonRequest request, PeopleSyncDDbContext db, TenantAccess tenant, CancellationToken cancellationToken) =>
        {
            if (!(await tenant.RequireMembershipAsync(http.User, request.OrganizationId, cancellationToken)).Allowed) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Email))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["person"] = ["First name, last name, and email are required."] });
            if (!await db.Organizations.AnyAsync(x => x.Id == request.OrganizationId, cancellationToken)) return Results.NotFound(new { message = "Organization was not found." });
            var email = request.Email.Trim().ToLowerInvariant();
            if (await db.People.AnyAsync(x => x.OrganizationId == request.OrganizationId && x.Email == email, cancellationToken)) return Results.Conflict(new { message = "A person with this email already exists in the organization." });
            var person = new PeopleSyncD.Domain.People.Person(Guid.NewGuid(), request.OrganizationId, request.FirstName, request.LastName, email);
            db.People.Add(person); await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/people/{person.Id}", new PersonResponse(person.Id, person.OrganizationId, person.FirstName, person.LastName, person.Email, person.Status.ToString()));
        });
        return endpoints;
    }
    public sealed record CreatePersonRequest(Guid OrganizationId, string FirstName, string LastName, string Email);
    public sealed record PersonResponse(Guid Id, Guid OrganizationId, string FirstName, string LastName, string Email, string Status);
}

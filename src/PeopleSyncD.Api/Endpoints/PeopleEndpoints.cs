using Microsoft.EntityFrameworkCore;
using PeopleSyncD.Domain.People;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Api.Endpoints;

public static class PeopleEndpoints
{
    public static IEndpointRouteBuilder MapPeopleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/people").WithTags("People");

        group.MapGet("/", async (Guid organizationId, PeopleSyncDDbContext db, CancellationToken cancellationToken) =>
            Results.Ok(await db.People.AsNoTracking().Where(x => x.OrganizationId == organizationId).OrderBy(x => x.LastName).Select(x => new PersonResponse(x.Id, x.OrganizationId, x.FirstName, x.LastName, x.Email, x.Status.ToString())).ToListAsync(cancellationToken)));

        group.MapPost("/", async (CreatePersonRequest request, PeopleSyncDDbContext db, CancellationToken cancellationToken) =>
        {
            if (request.OrganizationId == Guid.Empty || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Email))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["person"] = ["Organization, first name, last name, and email are required."] });

            var organizationExists = await db.Organizations.AnyAsync(x => x.Id == request.OrganizationId, cancellationToken);
            if (!organizationExists) return Results.NotFound(new { message = "Organization was not found." });

            var email = request.Email.Trim().ToLowerInvariant();
            var exists = await db.People.AnyAsync(x => x.OrganizationId == request.OrganizationId && x.Email == email, cancellationToken);
            if (exists) return Results.Conflict(new { message = "A person with this email already exists in the organization." });

            var person = new Person(Guid.NewGuid(), request.OrganizationId, request.FirstName, request.LastName, email);
            db.People.Add(person);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/v1/people/{person.Id}", new PersonResponse(person.Id, person.OrganizationId, person.FirstName, person.LastName, person.Email, person.Status.ToString()));
        });

        return endpoints;
    }

    public sealed record CreatePersonRequest(Guid OrganizationId, string FirstName, string LastName, string Email);
    public sealed record PersonResponse(Guid Id, Guid OrganizationId, string FirstName, string LastName, string Email, string Status);
}

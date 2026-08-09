using Microsoft.AspNetCore.Authorization;
using PeopleSyncD.Application.Identity;

namespace PeopleSyncD.Api.Endpoints;

public static class CurrentUserEndpoints
{
    public static IEndpointRouteBuilder MapCurrentUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/me", (ICurrentUserAccessor accessor) =>
        {
            var currentUser = accessor.GetCurrentUser();
            return currentUser is null ? Results.Unauthorized() : Results.Ok(currentUser);
        })
        .RequireAuthorization()
        .WithTags("Identity");

        return endpoints;
    }
}

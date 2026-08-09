namespace PeopleSyncD.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/auth/login", (LoginRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["credentials"] = ["Email and password are required."] });

            return Results.Problem(
                title: "Authentication provider not configured",
                detail: "The production identity provider must be configured before authentication is enabled.",
                statusCode: StatusCodes.Status501NotImplemented);
        }).WithTags("Authentication");

        return endpoints;
    }

    public sealed record LoginRequest(string Email, string Password);
}

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PeopleSyncD.Domain.Permissions;
using PeopleSyncD.Infrastructure.Configuration;

namespace PeopleSyncD.Api.Authentication;

/// <summary>
/// Configures JWT validation and tenant permission policies.
/// </summary>
public static class JwtAuthentication
{
    public const string TenantSelectedPolicy = "tenant.selected";

    public static IServiceCollection AddPlatformAuthentication(
        this IServiceCollection services,
        JwtOptions options)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization(authorization =>
        {
            authorization.AddPolicy(
                TenantSelectedPolicy,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim("tenant_id")
                    .RequireClaim("email_verified", "true"));
            AddPermissionPolicy(authorization, PermissionNames.OrganizationsRead);
            AddPermissionPolicy(authorization, PermissionNames.OrganizationsWrite);
            AddPermissionPolicy(authorization, PermissionNames.MembershipsRead);
            AddPermissionPolicy(authorization, PermissionNames.MembershipsWrite);
            AddPermissionPolicy(authorization, PermissionNames.EmployeesRead);
            AddPermissionPolicy(authorization, PermissionNames.EmployeesWrite);
        });
        return services;
    }

    private static void AddPermissionPolicy(
        Microsoft.AspNetCore.Authorization.AuthorizationOptions authorization,
        string permission)
    {
        authorization.AddPolicy(
            permission,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim("tenant_id")
                .RequireClaim("email_verified", "true")
                .RequireClaim("permission", permission));
    }
}

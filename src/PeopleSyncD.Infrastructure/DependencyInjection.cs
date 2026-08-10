using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Interfaces;
using PeopleSyncD.Infrastructure.Configuration;
using PeopleSyncD.Infrastructure.Identity;
using PeopleSyncD.Infrastructure.Persistence;
using PeopleSyncD.Infrastructure.Repositories;
using PeopleSyncD.Infrastructure.Telemetry;
using PeopleSyncD.SharedKernel;

namespace PeopleSyncD.Infrastructure;

/// <summary>
/// Registers persistence, identity, repositories, and infrastructure services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        JwtOptions jwtOptions)
    {
        var provider = configuration[$"{DatabaseOptions.SectionName}:Provider"] ?? "PostgreSql";
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (string.Equals(provider, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("PeopleSyncD.Tests");
                return;
            }

            var connectionString = configuration.GetConnectionString("peoplesyncd")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("A PostgreSQL connection string named 'peoplesyncd' is required.");
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var passkeyOptions = configuration.GetSection(PasskeyOptions.SectionName).Get<PasskeyOptions>()
            ?? new PasskeyOptions();
        var relyingPartyId = string.IsNullOrWhiteSpace(passkeyOptions.RelyingPartyId)
            ? throw new InvalidOperationException("WebAuthn relying-party ID is required.")
            : passkeyOptions.RelyingPartyId.Trim();
        var origins = passkeyOptions.Origins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (origins.Count == 0)
        {
            throw new InvalidOperationException("At least one WebAuthn origin is required.");
        }

        services.AddSingleton(passkeyOptions);
        services.AddSingleton<IFido2>(new Fido2(new Fido2Configuration
        {
            ServerDomain = relyingPartyId,
            ServerName = string.IsNullOrWhiteSpace(passkeyOptions.RelyingPartyName)
                ? "PeopleSyncD"
                : passkeyOptions.RelyingPartyName.Trim(),
            Origins = origins,
        }));
        services.AddSingleton(jwtOptions);
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationMembershipRepository, OrganizationMembershipRepository>();
        services.AddScoped<IOrganizationInvitationRepository, OrganizationInvitationRepository>();
        services.AddScoped<IIdentityGateway, IdentityGateway>();
        services.AddScoped<IIdentityAdministrationGateway, IdentityAdministrationGateway>();
        services.AddScoped<IMfaSecurityGateway, MfaSecurityGateway>();
        services.AddScoped<IPasskeySecurityGateway, PasskeySecurityGateway>();
        services.AddScoped<ITenantProvisioningGateway, TenantProvisioningGateway>();
        services.AddScoped<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddScoped<IRefreshSessionGateway, RefreshSessionGateway>();
        services.AddSingleton<IInvitationSecretService, InvitationSecretService>();
        services.AddScoped<IIdentityNotificationSender, DevelopmentFileIdentityNotificationSender>();
        services.AddScoped<IAuditRecorder, DatabaseAuditRecorder>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}

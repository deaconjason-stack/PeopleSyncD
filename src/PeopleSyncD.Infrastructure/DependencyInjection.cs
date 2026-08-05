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
        IConfiguration configuration)
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
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}

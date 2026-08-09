using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Infrastructure.Authorization;
using PeopleSyncD.Infrastructure.Persistence;

namespace PeopleSyncD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("peoplesyncd");
        if (!string.IsNullOrWhiteSpace(connectionString))
            services.AddDbContext<PeopleSyncDDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<TenantAccess>();
        return services;
    }
}

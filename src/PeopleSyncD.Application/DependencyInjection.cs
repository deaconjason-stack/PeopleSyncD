using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Organizations;

namespace PeopleSyncD.Application;

/// <summary>
/// Registers application-layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateOrganizationValidator>();
        services.AddScoped<CreateOrganizationService>();
        return services;
    }
}

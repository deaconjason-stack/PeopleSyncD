using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PeopleSyncD.Application.Identity;
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
        services.AddScoped<SessionTokenService>();
        services.AddScoped<RegisterTenantService>();
        services.AddScoped<LoginService>();
        services.AddScoped<ListOrganizationsService>();
        services.AddScoped<SelectOrganizationService>();
        services.AddScoped<InviteMemberService>();
        services.AddScoped<AcceptInvitationService>();
        services.AddScoped<ListMembersService>();
        services.AddScoped<UpdateMembershipService>();
        services.AddScoped<RequestEmailVerificationService>();
        services.AddScoped<ConfirmEmailService>();
        services.AddScoped<RefreshSessionService>();
        services.AddScoped<MfaSecurityService>();
        services.AddScoped<PasskeySecurityService>();
        services.AddScoped<PrivilegedAuthenticationPolicy>();
        services.AddScoped<SessionAdministrationService>();
        return services;
    }
}

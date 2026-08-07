using System.Globalization;
using System.Text.Json.Serialization;
using PeopleSyncD.Api.Authentication;
using PeopleSyncD.Api.Configuration;
using PeopleSyncD.Api.Middleware;
using PeopleSyncD.Application;
using PeopleSyncD.Infrastructure;
using PeopleSyncD.Infrastructure.Configuration;
using PeopleSyncD.Infrastructure.Persistence;
using PeopleSyncD.ServiceDefaults;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.UseSerilog((context, services, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

var allowEphemeralSigningKey = !builder.Environment.IsProduction();
var jwtOptions = JwtOptions.Create(builder.Configuration, allowEphemeralSigningKey);
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, jwtOptions);
builder.Services.AddPlatformAuthentication(jwtOptions);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseMiddleware<AccountSecurityValidationMiddleware>();
app.UseMiddleware<TenantMembershipValidationMiddleware>();
app.UseAuthorization();
app.MapOpenApi();
app.MapControllers();
app.MapDefaultEndpoints();

if (allowEphemeralSigningKey)
{
    await app.Services.InitializeDevelopmentDatabaseAsync();
}

app.Run();

public partial class Program;

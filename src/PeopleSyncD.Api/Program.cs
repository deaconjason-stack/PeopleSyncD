using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PeopleSyncD.Api.Endpoints;
using PeopleSyncD.Api.Identity;
using PeopleSyncD.Application.Identity;
using PeopleSyncD.Infrastructure;
using PeopleSyncD.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
builder.Services.AddAuthorization();

var authentication = builder.Configuration.GetSection("Authentication").Get<PeopleSyncD.Api.Identity.AuthenticationConfiguration>() ?? new();
if (!string.IsNullOrWhiteSpace(authentication.Authority) && !string.IsNullOrWhiteSpace(authentication.Audience))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authentication.Authority;
            options.Audience = authentication.Audience;
            options.RequireHttpsMetadata = authentication.RequireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });
}

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();

app.MapGet("/api", () => Results.Ok(new { name = "PeopleSyncD Enterprise Platform", version = "0.1.0-alpha", status = "operational" }));
app.MapGet("/version", () => Results.Ok(new { version = "0.1.0-alpha", build = "foundation" }));

app.MapAuthenticationEndpoints();
app.MapCurrentUserEndpoints();
app.MapOrganizationEndpoints();
app.MapPeopleEndpoints();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.Run();

public partial class Program;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace PeopleSyncD.ServiceDefaults;

internal static class Telemetry
{
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = uri);
                }
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = uri);
                }
            });

        return builder;
    }
}

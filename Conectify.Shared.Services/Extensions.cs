using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Conectify.Shared.Services;
public static class Extensions
{

    public static IServiceCollection AddTelemetry(this IServiceCollection service)
    {

        service.AddOpenTelemetry()
        .WithMetrics(x => x.AddRuntimeInstrumentation().AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "CustomMeters"))
        .WithTracing(x => x.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().SetSampler<AlwaysOnSampler>());

        service.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        service.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
        service.ConfigureOpenTelemetryTracerProvider(tracer => tracer.AddAspNetCoreInstrumentation().AddSource("CustomTracing").AddOtlpExporter());

        return service;
    }

    public static ILoggingBuilder AddRemoteLogging(this ILoggingBuilder builder)
    {
        return builder.AddOpenTelemetry(x =>
        {
            x.IncludeScopes = true;
            x.IncludeFormattedMessage = true;
        });
    }
}

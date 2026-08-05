using Conectify.Database;
using Conectify.Services.Android;
using Conectify.Services.Android.Middleware;
using Conectify.Services.Android.Services;
using Conectify.Services.Library;
using Conectify.Shared.Services;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ConectifyDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseString")));

builder.Services.AddScoped<AndroidWidgetService>();
builder.Services.AddScoped<AndroidConfigService>();

// Singleton: the whole point is that the snapshot outlives a request.
builder.Services.AddSingleton<WidgetCache>();

// Pooled client with a timeout well under the widget's one-minute refresh, so a History outage
// fails fast instead of holding the request open.
builder.Services.AddHttpClient<HistoryClient>(client => client.Timeout = TimeSpan.FromSeconds(5));

builder.Logging.AddRemoteLogging();

// UseConectifyWebsocket already calls AddTelemetry(); calling it again here registered a second
// OTLP exporter and a second set of instrumentations, so every span was reported twice in Aspire.
builder.Services.UseConectifyWebsocket<AndroidConfiguration, DeviceData>();

// Emit the widget's own spans (widget.build, actuator.trigger, history.*, widget.*.load).
builder.Services.ConfigureOpenTelemetryTracerProvider(tracer => tracer.AddSource(AndroidTelemetry.SourceName));

builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.ConnectToConectifyServer();

// Before everything else that handles /api/android, so the log line brackets the whole request.
app.UseMiddleware<ApiLoggingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapControllers();
app.Run();

using Conectify.Database;
using Conectify.Services.Android;
using Conectify.Services.Android.Services;
using Conectify.Services.Library;
using Conectify.Shared.Services;
using Microsoft.EntityFrameworkCore;

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
builder.Services.UseConectifyWebsocket<AndroidConfiguration, DeviceData>();
builder.Services.AddTelemetry();
builder.Services.AddHealthChecks();

var app = builder.Build();

await app.Services.ConnectToConectifyServer();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.UseHealthChecks("/health");
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapControllers();
app.Run();

using System.Timers;
using Conectify.Services.Library;
using Conectify.Services.Weather;
using Conectify.Shared.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddRemoteLogging();
builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();
builder.Services.AddTransient<WeatherFetcher>();

var app = builder.Build();

await app.Services.ConnectToConectifyServer();

app.UseHttpsRedirection();
app.UseHealthChecks("/health");

var configuration = app.Services.GetRequiredService<Configuration>();
var timer = new System.Timers.Timer(configuration.RefreshIntervalMinutes * 60 * 1000);
timer.Elapsed += Timer_ElapsedAsync;
timer.AutoReset = true;
timer.Enabled = true;

async void Timer_ElapsedAsync(object? sender, ElapsedEventArgs e)
{
    var fetcher = app.Services.GetRequiredService<WeatherFetcher>();
    await fetcher.LoadNewValues();
}

Timer_ElapsedAsync(null, null!);

app.Run();

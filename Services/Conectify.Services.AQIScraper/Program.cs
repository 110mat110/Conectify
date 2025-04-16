using System.Timers;
using Conectify.Services.AQIScraper;
using Conectify.Services.Library;
using Conectify.Shared.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddRemoteLogging();
builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();
builder.Services.AddTransient<CHMIScraper>();
// Add services to the container.

var app = builder.Build();

await app.Services.ConnectToConectifyServer();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseHealthChecks("/health");
var timer = new System.Timers.Timer(60 * 60 * 1000);
timer.Elapsed += Timer_ElapsedAsync;
timer.AutoReset = true;
timer.Enabled = true;

async void Timer_ElapsedAsync(object? sender, ElapsedEventArgs e)
{
    var scraper = app.Services.GetRequiredService<CHMIScraper>();
    await scraper.LoadNewValues();
}

Timer_ElapsedAsync(null, null!);

app.Run();
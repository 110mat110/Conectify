using Conectify.Services.Library;
using Conectify.Services.AQIScraper;
using System.Timers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.UseConectifyWebsocket<Conectify.Services.AQIScraper.Configuration, DeviceData>();
builder.Services.AddTransient<CHMIScraper>();
// Add services to the container.

var app = builder.Build();

await app.Services.ConnectToConectifyServer();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var timer = new System.Timers.Timer(3*60*1000*1000);
timer.Elapsed += Timer_ElapsedAsync;
timer.AutoReset = true;
timer.Start();

async void Timer_ElapsedAsync(object? sender, ElapsedEventArgs e)
{
    var scraper = app.Services.GetRequiredService<CHMIScraper>();
    await scraper.LoadNewValues();
}

Timer_ElapsedAsync(null, null!);

app.Run();
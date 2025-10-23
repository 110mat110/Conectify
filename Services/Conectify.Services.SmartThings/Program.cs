using Conectify.Database;
using Conectify.Services.SmartThings;
using Conectify.Services.SmartThings.Services;
using Microsoft.EntityFrameworkCore;
using Conectify.Services.Library;
using System.Timers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ConectifyDb>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DatabaseString")));
builder.Services.AddScoped<SmartThingsService>();
builder.Services.AddScoped<SmartThingsAuthService>();
builder.Services.UseConectifyWebsocket<SmartThingsConfiguration, DeviceData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
var timer = new System.Timers.Timer(60 * 1000);
timer.Elapsed += Timer_ElapsedAsync;
timer.AutoReset = true;
timer.Enabled = true;

async void Timer_ElapsedAsync(object? sender, ElapsedEventArgs e)
{
    var scope = app.Services.CreateScope();
    var service = scope.ServiceProvider.GetRequiredService<SmartThingsService>();
    await service.RefreshAllCapabilities(default);
}

var scope = app.Services.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<SmartThingsService>();
await service.RegisterAllDevices(default);

Timer_ElapsedAsync(null, null!);
app.UseHealthChecks("/health");
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Run();

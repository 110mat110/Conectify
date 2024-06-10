using Conectify.Services.OccupancyCheck;
using Conectify.Services.Library;
using Conectify.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Logging.AddRemoteLogging();
builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();
builder.Services.AddTransient<OccupancyService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
await app.Services.ConnectToConectifyServer();

var scraper = app.Services.GetRequiredService<OccupancyService>();
scraper.CheckForLiveDevices();

app.Run();
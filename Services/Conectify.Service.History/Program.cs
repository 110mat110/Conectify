using Conectify.Database;
using Conectify.Service.History;
using Conectify.Service.History.Services;
using Conectify.Services.Library;
using Conectify.Shared.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddRemoteLogging();
builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();
builder.Services.AddSingleton<IDataCachingService, DataCachingService>();
builder.Services.AddSingleton<IDeviceCachingService, DeviceCachingService>();
builder.Services.AddDbContext<ConectifyDb>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DatabaseString")));

builder.Services.AddTelemetry();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
await app.Services.ConnectToConectifyServer();
var ws = app.Services.GetRequiredService<IServicesWebsocketClient>();
ws.OnIncomingEvent += async (ws_IncomingValue) =>
{
    var dataCache = app.Services.GetRequiredService<IDataCachingService>();
    var deviceCache = app.Services.GetRequiredService<IDeviceCachingService>();
    await dataCache.InsertValue(ws_IncomingValue);
    deviceCache.ObserveSensorFromEvent(ws_IncomingValue);
};

app.MapControllers();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Run();

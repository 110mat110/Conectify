using Conectify.Services.Library;
using Conectify.Services.ShellyConnector;
using Conectify.Services.ShellyConnector.Services;
using Conectify.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Logging.AddRemoteLogging();
builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();
builder.Services.AddTransient<IShellyService, ShellyService>();
builder.Services.AddHostedService<ValueScraper>();
var app = builder.Build();

await app.Services.ConnectToConectifyServer();
app.Services.GetRequiredService<IServicesWebsocketClient>().OnIncomingAction += OnEvent;
async void OnEvent(Conectify.Database.Models.Values.Action action)
{
    var shellyService = app.Services.GetRequiredService<IShellyService>();
    await shellyService.SendValueToShelly(action);
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseWebSockets();
app.MapControllers();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Run();

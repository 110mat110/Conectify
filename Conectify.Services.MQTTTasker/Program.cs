using Conectify.Services.Library;
using Conectify.Services.MQTTTasker;
using Conectify.Services.MQTTTasker.Services;
using Conectify.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Logging.AddRemoteLogging();
builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();
builder.Services.AddSingleton<IMQTTSender, MQTTSender>();
builder.Services.AddSingleton<MqttService>();
builder.Services.AddTransient<IValueService, ValueService>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.Services.ConnectToConectifyServer();
app.Services.GetRequiredService<IServicesWebsocketClient>().OnIncomingEvent += OnEvent;
async void OnEvent(Conectify.Database.Models.Values.Event evnt)
{
    //var shellyService = app.Services.GetRequiredService<IMQTTSender>();
    //await shellyService.SendValueToBroker(evnt, CancellationToken.None);
}

var mqttSevice = app.Services.GetService<MqttService>();
mqttSevice.StartAsync(default);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

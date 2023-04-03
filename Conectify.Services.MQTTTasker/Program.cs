using Conectify.Services.Library;
using Conectify.Services.MQTTTasker;
using Conectify.Services.MQTTTasker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.UseConectifyWebsocket<Conectify.Services.MQTTTasker.Configuration, DeviceData>();
builder.Services.AddSingleton<IMQTTSender, MQTTSender>();
builder.Services.AddTransient<IValueService, ValueService>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.Services.ConnectToConectifyServer();
app.Services.GetRequiredService<IServicesWebsocketClient>().OnIncomingValue += OnEvent;
async void OnEvent(Conectify.Database.Models.Values.Value action)
{
	var shellyService = app.Services.GetRequiredService<IMQTTSender>();
	await shellyService.SendValueToBroker(action, CancellationToken.None);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

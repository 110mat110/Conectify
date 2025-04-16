using Conectify.Services.Library;
using Conectify.Services.Pushover;
using Conectify.Shared.Library;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.UseConectifyWebsocket<Configuration, DeviceData>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
var ws = app.Services.GetRequiredService<IServicesWebsocketClient>();
ws.OnIncomingEvent += async (ws_Event) =>
{
    var config = app.Services.GetRequiredService<Configuration>();

    if (config.SensorId == ws_Event.DestinationId)
    {
        await Tracing.Trace(async () =>
        {
            var parameters = new Dictionary<string, string>
            {
                ["token"] = config.Token,
                ["user"] = config.ClientKey,
                ["message"] = ws_Event.StringValue,
            };
            using var client = new HttpClient();
            var response = await client.PostAsync("https://api.pushover.net/1/messages.json", new
            FormUrlEncodedContent(parameters));
        }, ws_Event.Id, "Sending to pushover");
    }
};


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

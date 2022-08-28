// See https://aka.ms/new-console-template for more information
using Conectify.Database.Models.Values;
using Conectify.Services.Library;
using TestService;

var config = new LocalConfig();
var client = new ServicesWebsocketClient();
client.OnIncomingValue += Client_OnIncomingValue;

await client.ConnectAsync(config.TargetIp + "api/Websocket/test/test");

Console.ReadKey();

await client.DisconnectAsync();
client.Dispose();
Console.ReadKey();

void Client_OnIncomingValue(Value value)
{
    Console.WriteLine(value.Name);
}
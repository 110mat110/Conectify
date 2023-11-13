using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Conectify.Services.ShellyConnector.Services;

public class ValueScraper : BackgroundService
{
    private readonly Configuration configuration;
    private readonly IServicesWebsocketClient websocketClient;

    public ValueScraper(Configuration configuration, IServicesWebsocketClient websocketClient)
    {
        this.configuration = configuration;
        this.websocketClient = websocketClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (configuration.ShellyType.ToLower().Contains("pm"))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

                string res = await client.GetStringAsync($"{configuration.ShellyIp}/rpc/Switch.GetStatus?id=0", stoppingToken);

                var powerReading = JsonConvert.DeserializeAnonymousType(res, new { apower = float.NaN });
                if (powerReading != null && !float.IsNaN(powerReading.apower))
                {
                    await websocketClient.SendMessageAsync(new WebsocketBaseModel()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Power",
                        NumericValue = powerReading.apower,
                        SourceId = configuration.PowerSensorId,
                        TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        Type = Constants.Types.Value,
                        Unit = "W",
                        StringValue = string.Empty
                    }, stoppingToken);
                }

                await Task.Delay(new TimeSpan(0, 1, 0), stoppingToken);
            }
        }

		if (configuration.ShellyType.ToLower().Contains("plug"))
        {
			while (!stoppingToken.IsCancellationRequested)
			{
				using var client = new HttpClient();
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

				string res = await client.GetStringAsync($"{configuration.ShellyIp}/meter/0", stoppingToken);

				var powerReading = JsonConvert.DeserializeAnonymousType(res, new { power = float.NaN });
				if (powerReading != null && !float.IsNaN(powerReading.power))
				{
					await websocketClient.SendMessageAsync(new WebsocketBaseModel()
					{
						Id = Guid.NewGuid(),
						Name = "Power",
						NumericValue = powerReading.power,
						SourceId = configuration.PowerSensorId,
						TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
						Type = Constants.Types.Value,
						Unit = "W",
						StringValue = string.Empty
					}, stoppingToken);
				}

				await Task.Delay(new TimeSpan(0, 1, 0), stoppingToken);
			}
		}

	}
}

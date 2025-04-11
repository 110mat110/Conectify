using Conectify.Database.Interfaces;
using MQTTnet.Client;
using MQTTnet;
using Conectify.Database.Models.Values;

namespace Conectify.Services.MQTTTasker.Services;

public interface IMQTTSender
{
	Task SendValueToBroker(Event input, CancellationToken cancellationToken);
}

internal class MQTTSender(Configuration configuration) : IMQTTSender
{
    public async Task SendValueToBroker(Event input, CancellationToken cancellationToken)
	{
		if (!input.NumericValue.HasValue)
		{
			return;
		}

		var mqttFactory = new MqttFactory();

        using var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(configuration.Broker)
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);

        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(input.SourceId.ToString())
            .WithPayload(input.NumericValue.Value.ToString())
            .Build();

        await mqttClient.PublishAsync(applicationMessage, cancellationToken);

        await mqttClient.DisconnectAsync();
    }
}

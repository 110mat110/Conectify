using Conectify.Database.Interfaces;
using MQTTnet.Client;
using MQTTnet;

namespace Conectify.Services.MQTTTasker.Services;

public interface IMQTTSender
{
	Task SendValueToBroker(IBaseInputType input, CancellationToken cancellationToken);
}

internal class MQTTSender : IMQTTSender
{
	private readonly Configuration configuration;

	public MQTTSender(Configuration configuration)
	{
		this.configuration = configuration;
	}

	public async Task SendValueToBroker(IBaseInputType input, CancellationToken cancellationToken)
	{
		if (!input.NumericValue.HasValue)
		{
			return;
		}

		var mqttFactory = new MqttFactory();

		using (var mqttClient = mqttFactory.CreateMqttClient())
		{
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
}

using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;

namespace Conectify.Services.MQTTTasker.Services;

public interface IValueService
{
	Task<bool> SetAction(Guid actuatorId, float  value);
}

internal class ValueService(IServicesWebsocketClient websocketClient, IDeviceData deviceData) : IValueService
{
    public async Task<bool> SetAction(Guid actuatorId, float value)
	{
		var response = new WebsocketBaseModel()
		{
			Id = Guid.NewGuid(),
			DestinationId = actuatorId,
			Name = "SetFromTasker",
			NumericValue = value,
			SourceId = deviceData.Sensors.First().Id,
			TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
			Unit = string.Empty,
			Type = Constants.Types.Action,
			StringValue = string.Empty
		};

		return await websocketClient.SendMessageAsync(response);
	}
}

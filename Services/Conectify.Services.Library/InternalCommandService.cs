using Conectify.Database.Models.Values;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Library;

public interface IInternalCommandService
{
    Task<bool> HandleInternalCommand(Event command, CancellationToken ct);
}
internal class InternalCommandService(ConfigurationBase configuration, IServiceProvider serviceProvider) : IInternalCommandService
{
    public async Task<bool> HandleInternalCommand(Event command, CancellationToken ct)
    {
        if (command.DestinationId != configuration.DeviceId)
        {
            return false;
        }
        switch (command.Name)
        {
            case Constants.Commands.ActivityCheck: await SendActivityResponse(command, ct); return true;
            // Our own probe echoed back by the server. Receiving it is the whole point, the watchdog
            // has already been told about it - it just has no business in the service itself.
            case Constants.Commands.ConnectionCheck: return true;
            default: return false;
        }
    }

    private async Task SendActivityResponse(Event command, CancellationToken ct)
    {
        var response = new WebsocketEvent()
        {
            Name = Constants.Commands.Active,
            NumericValue = 1,
            SourceId = command.Id,
            StringValue = string.Empty,
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Type = Constants.Events.CommandResponse,
        };
        var websocketClient = serviceProvider.GetRequiredService<IServicesWebsocketClient>();
        await websocketClient.SendMessageAsync(response, ct);
    }
}

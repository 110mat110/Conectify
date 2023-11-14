using Conectify.Database.Models.Values;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.Extensions.DependencyInjection;

namespace Conectify.Services.Library;

public interface IInternalCommandService
{
    Task<bool> HandleInternalCommand(Command command, CancellationToken ct);
}
internal class InternalCommandService : IInternalCommandService
{
    private readonly Configuration configuration;
    private readonly IServiceProvider serviceProvider;

    public InternalCommandService(Configuration configuration, IServiceProvider serviceProvider)
    {
        this.configuration = configuration;
        this.serviceProvider = serviceProvider;
    }
    public async Task<bool> HandleInternalCommand(Command command, CancellationToken ct)
    {
        if (command.DestinationId != configuration.DeviceId)
        {
            return false;
        }
        switch (command.Name)
        {
            case Constants.Commands.ActivityCheck: await SendActivityResponse(command, ct); return true;
            default: return false;
        }
    }

    private async Task SendActivityResponse(Command command, CancellationToken ct)
    {
        var response = new WebsocketBaseModel()
        {
            Name = Constants.Commands.Active,
            NumericValue = 1,
            SourceId = configuration.DeviceId,
            StringValue = string.Empty,
            TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Type = Constants.Types.CommandResponse,
            ResponseSourceId = command.Id
        };
        var websocketClient = serviceProvider.GetRequiredService<IServicesWebsocketClient>();
        await websocketClient.SendMessageAsync(response, ct);
    }
}

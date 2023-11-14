using AutoMapper;
using Conectify.Database.Interfaces;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Library.Models.Websocket;
using Conectify.Shared.Services.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace Conectify.Services.Library;

public interface IServicesWebsocketClient
{
    event IncomingActionDelegate? OnIncomingAction;
    event IncomingActionResponseDelegate? OnIncomingActionResponse;
    event IncomingCommandDelegate? OnIncomingCommand;
    event IncomingCommandResponseDelegate? OnIncomingCommandResponse;
    event IncomingValueDelegate? OnIncomingValue;

    Task ConnectAsync();
    Task ConnectAsync(string url);
    Task DisconnectAsync();
    Task<bool> SendMessageAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default) where TRequest : IWebsocketModel;
}

public class ServicesWebsocketClient : IServicesWebsocketClient
{
    public ServicesWebsocketClient(Configuration configuration, IMapper mapper, ILogger<ServicesWebsocketClient> logger)
    {
        this.configuration = configuration;
        this.mapper = mapper;
        this.logger = logger;
        var timer = new System.Timers.Timer(2000);
        timer.Elapsed += Timer_Elapsed;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await ConnectAsync();
    }

    public int ReceiveBufferSize { get; set; } = 8192;

    public event IncomingValueDelegate? OnIncomingValue;
    public event IncomingActionDelegate? OnIncomingAction;
    public event IncomingCommandDelegate? OnIncomingCommand;
    public event IncomingActionResponseDelegate? OnIncomingActionResponse;
    public event IncomingCommandResponseDelegate? OnIncomingCommandResponse;

    public async Task ConnectAsync()
    {
        await ConnectAsync(($"{configuration.WebsocketUrl}/api/Websocket/{configuration.DeviceId}"));
    }

    public async Task ConnectAsync(string url)
    {
        if (WS != null)
        {
            if (WS.State == WebSocketState.Open) return;
            else WS.Dispose();
        }
        logger.LogInformation("Trying to reconnect websocket");
        WS = new ClientWebSocket();
        if (CTS != null)
        {
            CTS.Cancel();
            CTS.Dispose();
        }
        CTS = new CancellationTokenSource();
        try
        {
            await WS.ConnectAsync(new Uri(url), CTS.Token);
            if (CTS.IsCancellationRequested) return;
            logger.LogWarning("Connected to websocket!");
            await Task.Factory.StartNew(ReceiveLoop, CTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        catch (Exception) { };
    }

    public async Task DisconnectAsync()
    {
        if (WS is null) return;
        // TODO: requests cleanup code, sub-protocol dependent.
        if (WS.State == WebSocketState.Open)
        {
            CTS?.CancelAfter(TimeSpan.FromSeconds(0));
            await WS.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
            await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
        WS.Dispose();
        WS = null;
        CTS?.Dispose();
        CTS = null;
    }

    private async Task ReceiveLoop()
    {
        var loopToken = CTS?.Token;
        var buffer = new byte[ReceiveBufferSize];
        try
        {
            while (WS is not null && loopToken is not null && !loopToken.Value.IsCancellationRequested)
            {
                using var outputStream = new MemoryStream(ReceiveBufferSize);
                WebSocketReceiveResult? receiveResult = null;
                do
                {
                    receiveResult = await WS.ReceiveAsync(buffer, loopToken.Value);
                    if (receiveResult.MessageType != WebSocketMessageType.Close)
                        outputStream.Write(buffer, 0, receiveResult.Count);
                }
                while (!receiveResult.EndOfMessage);
                if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                outputStream.Position = 0;
                await ResponseReceived(outputStream, default);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("WS failed!");
            logger.LogError(ex.Message);
        }
        finally
        {
            logger.LogWarning("Closing ws");
            //outputStream?.Dispose();
        }
        await ConnectAsync();
    }

    public async Task<bool> SendMessageAsync<RequestType>(RequestType message, CancellationToken cancellationToken = default) where RequestType : IWebsocketModel
    {
        await ConnectAsync();

        if (WS is null)
        {
            logger.LogError("Websocket unavaliable");
            return false;
        }
        var msg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        logger.LogInformation("Sending message via websocket: {msg}", message);
        await WS.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, cancellationToken);
        return true;
    }

    private async Task ResponseReceived(Stream inputStream, CancellationToken ct)
    {
        StreamReader stream = new(inputStream);
        string serializedMessage = stream.ReadToEnd();
        stream.Dispose();

        logger.LogInformation("WS recieved message {serializedMessage}", serializedMessage);
        var (entity, _) = SharedDataService.DeserializeJson(serializedMessage, this.mapper);
        await NotifyAboutIncomingMessage(entity, ct);
    }


    private ClientWebSocket? WS;
    private CancellationTokenSource? CTS;
    private readonly Configuration configuration;
    private readonly IMapper mapper;
    private readonly ILogger<ServicesWebsocketClient> logger;

    private async Task NotifyAboutIncomingMessage(IBaseInputType inputEntity, CancellationToken ct)
    {
        if (inputEntity is Value inputValue)
        {
            OnIncomingValue?.Invoke(inputValue);
        }

        if (inputEntity is Database.Models.Values.Action action)
        {
            OnIncomingAction?.Invoke(action);
        }

        if (inputEntity is Command command)
        {
            if (!await HandleInternalCommand(command, ct))
            {
                OnIncomingCommand?.Invoke(command);
            }
        }

        if (inputEntity is ActionResponse actionResponse)
        {
            OnIncomingActionResponse?.Invoke(actionResponse);
        }

        if (inputEntity is CommandResponse commandResponse)
        {
            OnIncomingCommandResponse?.Invoke(commandResponse);
        }
    }

    private async Task<bool> HandleInternalCommand(Command command, CancellationToken ct)
    {
        switch (command.Name)
        {
            case Constants.Commands.ActivityCheck: await SendActivityResponse(command,ct); return false;
            default: return true;
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
        await SendMessageAsync(response,ct);
    }
}

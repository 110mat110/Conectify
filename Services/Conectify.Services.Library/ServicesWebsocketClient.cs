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
    event IncomingEventDelegate? OnIncomingEvent;

    Task ConnectAsync();
    Task ConnectAsync(string url);
    Task DisconnectAsync();
    Task<bool> SendMessageAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default) where TRequest : IWebsocketModel;
}

public class ServicesWebsocketClient : IServicesWebsocketClient
{
    public ServicesWebsocketClient(IInternalCommandService internalCommandService, ConfigurationBase configuration, IMapper mapper, ILogger<ServicesWebsocketClient> logger)
    {
        this.internalCommandService = internalCommandService;
        this.configuration = configuration;
        this.mapper = mapper;
        this.logger = logger;
        var timer = new System.Timers.Timer(2000);
        timer.Elapsed += Timer_Elapsed;
        timer.AutoReset = true;
        //timer.Enabled = true;
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await ConnectAsync();
    }

    public int ReceiveBufferSize { get; set; } = 8192;

    public event IncomingEventDelegate? OnIncomingEvent;

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
            while (WS is not null && WS.State == WebSocketState.Open && loopToken is not null && !loopToken.Value.IsCancellationRequested)
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

        var evnt = SharedDataService.DeserializeJson(serializedMessage, this.mapper);

        if (evnt is null)
        {
            logger.LogError("Received invalid event {serializedMessage}", serializedMessage);
            return;
        }
       

        if (evnt.Id == Guid.Empty)
        {
            evnt.Id = Guid.NewGuid();
        }

        try
        {
            await Tracing.Trace(async () =>
            {

                logger.LogInformation("WS recieved message {serializedMessage}", serializedMessage);
                await NotifyAboutIncomingMessage(evnt, ct);
            }, evnt.Id, "Received event from WS");
        }
        catch (Exception ex)
        {
            logger.LogError("Event processing failed");
            logger.LogError(ex, ex.Message);
        }
    }


    private ClientWebSocket? WS;
    private CancellationTokenSource? CTS;
    private readonly IInternalCommandService internalCommandService;
    private readonly ConfigurationBase configuration;
    private readonly IMapper mapper;
    private readonly ILogger<ServicesWebsocketClient> logger;

    private async Task NotifyAboutIncomingMessage(Event evnt, CancellationToken ct)
    {
        bool handled = false;
        if(evnt.Type == Constants.Events.Command)
        {
            handled = await internalCommandService.HandleInternalCommand(evnt, ct);
        };

        if (!handled) {
            OnIncomingEvent?.Invoke(evnt);
        }
    }
}

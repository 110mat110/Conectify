using System.Net.WebSockets;
using System.Text;
using AutoMapper;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Services.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Conectify.Services.Library;

public interface IServicesWebsocketClient
{
    event IncomingEventDelegate? OnIncomingEvent;

    Task ConnectAsync();
    Task ConnectAsync(string url);
    Task DisconnectAsync();

    /// <summary>
    /// Drops the current socket without the closing handshake and builds a new one. For when the
    /// socket claims to be open but the connection behind it is dead - a graceful close would wait
    /// for an answer that is never coming.
    /// </summary>
    Task ForceReconnectAsync(CancellationToken cancellationToken = default);

    Task<bool> SendMessageAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default) where TRequest : IWebsocketModel;

    WebSocketState WebSocketState { get; }
}

public class ServicesWebsocketClient : IServicesWebsocketClient
{
    public ServicesWebsocketClient(IInternalCommandService internalCommandService, ConfigurationBase configuration, IConnectionState connectionState, IMapper mapper, ILogger<ServicesWebsocketClient> logger)
    {
        this.internalCommandService = internalCommandService;
        this.configuration = configuration;
        this.connectionState = connectionState;
        this.mapper = mapper;
        this.logger = logger;
        var timer = new System.Timers.Timer(2000);
        timer.Elapsed += Timer_Elapsed;
        timer.AutoReset = true;
        timer.Start();
    }

    private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        await ConnectAsync();
    }

    public int ReceiveBufferSize { get; set; } = 8192;

    public WebSocketState WebSocketState => WS?.State ?? WebSocketState.None;

    public event IncomingEventDelegate? OnIncomingEvent;

    public async Task ConnectAsync()
    {
        await ConnectAsync(($"{configuration.WebsocketUrl}/api/Websocket/{configuration.DeviceId}"));
    }

    public async Task ConnectAsync(string url)
    {
        // The reconnect timer, the receive loop and the watchdog all end up here; without the lock
        // they can build several sockets at once and leave the one nobody listens on behind.
        await connectionLock.WaitAsync();
        try
        {
            if (WS != null)
            {
                if (WS.State == WebSocketState.Open) return;
                else WS.Dispose();
            }
            logger.LogInformation("Trying to reconnect websocket");
            WS = new ClientWebSocket();
            WS.Options.KeepAliveInterval = KeepAliveInterval;
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
                // A fresh connection is proof of life on its own, and it stops the watchdog from
                // probing a connection that is only a moment old.
                connectionState.ReportContact();
                await Task.Factory.StartNew(ReceiveLoop, CTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (Exception) { }
            ;
        }
        finally
        {
            connectionLock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await connectionLock.WaitAsync();
        try
        {
            if (WS is null) return;
            // TODO: requests cleanup code, sub-protocol dependent.
            if (WS.State == WebSocketState.Open)
            {
                CTS?.CancelAfter(TimeSpan.FromSeconds(0));
                try
                {
                    // The closing handshake waits for the other side to answer, which never happens
                    // when the connection is already gone - so it only gets a limited time.
                    using var closing = new CancellationTokenSource(CloseTimeout);
                    await WS.CloseOutputAsync(WebSocketCloseStatus.Empty, "", closing.Token);
                    await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "", closing.Token);
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Could not close websocket gracefully: {message}", ex.Message);
                    WS.Abort();
                }
            }
            WS.Dispose();
            WS = null;
            CTS?.Dispose();
            CTS = null;
        }
        finally
        {
            connectionLock.Release();
        }
    }

    public async Task ForceReconnectAsync(CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Forcing websocket reconnect");
        await connectionLock.WaitAsync(cancellationToken);
        try
        {
            CTS?.Cancel();
            WS?.Abort();
            WS?.Dispose();
            WS = null;
            CTS?.Dispose();
            CTS = null;
        }
        finally
        {
            connectionLock.Release();
        }

        await ConnectAsync();
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
            logger.LogError("WS failed! {message}", ex.Message);
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
        try
        {
            await WS.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (Exception ex)
        {
            // A failing send is how a broken connection shows up first; report it instead of throwing
            // it at whoever happened to be sending, the watchdog turns it into a reconnect.
            logger.LogError("Could not send message via websocket: {message}", ex.Message);
            return false;
        }
        return true;
    }

    private async Task ResponseReceived(Stream inputStream, CancellationToken ct)
    {
        connectionState.ReportContact();

        StreamReader stream = new(inputStream);
        string serializedMessage = stream.ReadToEnd();
        stream.Dispose();

        var evnt = SharedDataService.DeserializeJson(serializedMessage);

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
            logger.LogError("Event processing failed {message}", ex.Message);
        }
    }


    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);

    private ClientWebSocket? WS;
    private CancellationTokenSource? CTS;
    private readonly SemaphoreSlim connectionLock = new(1, 1);
    private readonly IInternalCommandService internalCommandService;
    private readonly ConfigurationBase configuration;
    private readonly IConnectionState connectionState;
    private readonly IMapper mapper;
    private readonly ILogger<ServicesWebsocketClient> logger;

    private async Task NotifyAboutIncomingMessage(Event evnt, CancellationToken ct)
    {
        bool handled = false;
        if (evnt.Type == Constants.Events.Command)
        {
            handled = await internalCommandService.HandleInternalCommand(evnt, ct);
        }
        ;

        if (!handled)
        {
            OnIncomingEvent?.Invoke(evnt);
        }
    }
}

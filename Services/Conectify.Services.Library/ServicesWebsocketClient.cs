using AutoMapper;
using Conectify.Database.Interfaces;
using Conectify.Database.Models.Values;
using Conectify.Shared.Library.Interfaces;
using Conectify.Shared.Services.Data;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace Conectify.Services.Library
{
    public interface IServicesWebsocketClient
    {
        event IncomingActionDelegate OnIncomingAction;
        event IncomingActionResponseDelegate OnIncomingActionResponse;
        event IncomingCommandDelegate OnIncomingCommand;
        event IncomingCommandResponseDelegate OnIncomingCommandResponse;
        event IncomingValueDelegate OnIncomingValue;

        Task ConnectAsync();
        Task ConnectAsync(string url);
        Task DisconnectAsync();
        Task<bool> SendMessageAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default) where TRequest : IWebsocketModel;
    }

    public class ServicesWebsocketClient : IServicesWebsocketClient
    {
        public ServicesWebsocketClient(Configuration configuration, IMapper mapper)
        {
            this.configuration = configuration;
            this.mapper = mapper;
        }

        public int ReceiveBufferSize { get; set; } = 8192;

        public event IncomingValueDelegate OnIncomingValue;
        public event IncomingActionDelegate OnIncomingAction;
        public event IncomingCommandDelegate OnIncomingCommand;
        public event IncomingActionResponseDelegate OnIncomingActionResponse;
        public event IncomingCommandResponseDelegate OnIncomingCommandResponse;

        public async Task ConnectAsync()
        {
            //await ConnectAsync(($"{configuration.WebsocketUrl}/api/Websocket/test/test"));
            await ConnectAsync(($"{configuration.WebsocketUrl}/api/Websocket/{configuration.DeviceId}"));
        }

        public async Task ConnectAsync(string url)
        {
            if (WS != null)
            {
                if (WS.State == WebSocketState.Open) return;
                else WS.Dispose();
            }
            WS = new ClientWebSocket();
            if (CTS != null) CTS.Dispose();
            CTS = new CancellationTokenSource();
            await WS.ConnectAsync(new Uri(url), CTS.Token);
            //await ReceiveLoop();
            await Task.Factory.StartNew(ReceiveLoop, CTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public async Task DisconnectAsync()
        {
            if (WS is null) return;
            // TODO: requests cleanup code, sub-protocol dependent.
            if (WS.State == WebSocketState.Open)
            {
                CTS.CancelAfter(TimeSpan.FromSeconds(0));
                await WS.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            WS.Dispose();
            WS = null;
            CTS.Dispose();
            CTS = null;
        }

        private async Task ReceiveLoop()
        {
            var loopToken = CTS.Token;
            MemoryStream outputStream = null;
            WebSocketReceiveResult receiveResult = null;
            var buffer = new byte[ReceiveBufferSize];
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    outputStream = new MemoryStream(ReceiveBufferSize);
                    do
                    {
                        receiveResult = await WS.ReceiveAsync(buffer, CTS.Token);
                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                            outputStream.Write(buffer, 0, receiveResult.Count);
                    }
                    while (!receiveResult.EndOfMessage);
                    if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                    outputStream.Position = 0;
                    ResponseReceived(outputStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WS failed!");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Closing ws");
                outputStream?.Dispose();
            }
        }

        public async Task<bool> SendMessageAsync<RequestType>(RequestType message, CancellationToken cancellationToken = default) where RequestType : IWebsocketModel
        {
            var msg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await WS.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, cancellationToken);
            return true;
        }

        private void ResponseReceived(Stream inputStream)
        {
            StreamReader stream = new StreamReader(inputStream);
            string x = stream.ReadToEnd();
            stream.Dispose();

            var (entity, type) = SharedDataService.DeserializeJson(x, this.mapper);
            NotifyAboutIncomingMessage(entity, type);
        }


        private ClientWebSocket WS;
        private CancellationTokenSource CTS;
        private readonly Configuration configuration;
        private readonly IMapper mapper;

        private void NotifyAboutIncomingMessage(IBaseInputType inputEntity, Type entitytype)
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
                OnIncomingCommand?.Invoke(command);
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

    }
}

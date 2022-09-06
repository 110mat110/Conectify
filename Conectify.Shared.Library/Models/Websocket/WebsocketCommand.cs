namespace Conectify.Shared.Library.Models.Websocket;

using System;

public class WebsocketCommand : WebsocketBaseModel
{
    public Guid DestinationId { get; set; }
}

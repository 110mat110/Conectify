namespace Conectify.Shared.Library.Models.Websocket;

using System;

public class WebsocketCommandResponse : WebsocketBaseModel
{
    public Guid CommandId { get; set; }
}

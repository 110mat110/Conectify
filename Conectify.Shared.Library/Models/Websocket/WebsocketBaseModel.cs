﻿namespace Conectify.Shared.Library.Models.Websocket;

using Conectify.Shared.Library.Classes;
using Conectify.Shared.Library.Interfaces;
using System;

public interface IWebsocketBaseModel : ISerializable, IWebsocketModel
{
    Guid Id { get; set; }
    string Type { get; set; }
    Guid SourceId { get; set; }
    string Name { get; set; }
    string Unit { get; set; }
    string StringValue { get; set; }
    float? NumericValue { get; set; }
    long TimeCreated { get; set; }
}

public class WebsocketBaseModel : Serializable, IWebsocketBaseModel
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string StringValue { get; set; } = string.Empty;
    public float? NumericValue { get; set; }
    public long TimeCreated { get; set; }
    public Guid SourceId { get; set; }
}

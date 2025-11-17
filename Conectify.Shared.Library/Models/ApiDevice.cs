namespace Conectify.Shared.Library.Models;

using System;
using System.Collections.Generic;
using Conectify.Shared.Library.Interfaces;

public enum ApiDeviceState
{
    Offline = 0,
    NotAnswering = 1,
    Online = 2,
}

public record ApiDevice : IApiModel
{
    public Guid Id { get; set; }

    public string IPAdress { get; set; } = string.Empty;
    public string MacAdress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ApiDeviceState State { get; set; } = ApiDeviceState.Offline;
    public virtual List<ApiMetadata> Metadata { get; set; } = [];
}

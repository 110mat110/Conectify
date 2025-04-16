namespace Conectify.Shared.Library.Models;

using System;
using System.Collections.Generic;
using Conectify.Shared.Library.Interfaces;

public record ApiActuator : IApiModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SourceDeviceId { get; set; }
    public Guid SensorId { get; set; }

    public virtual List<ApiMetadata> Metadata { get; set; } = [];
}

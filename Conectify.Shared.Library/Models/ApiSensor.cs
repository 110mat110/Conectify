namespace Conectify.Shared.Library.Models;

using System;
using System.Collections.Generic;
using Conectify.Shared.Library.Interfaces;

public record ApiSensor : IApiModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public virtual List<ApiMetadata> Metadata { get; set; } = [];

    public Guid SourceDeviceId { get; set; }
}

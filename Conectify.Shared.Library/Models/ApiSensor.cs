namespace Conectify.Shared.Library.Models;

using Conectify.Shared.Library.Interfaces;
using System;
using System.Collections.Generic;

public record ApiSensor : IApiModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public virtual List<ApiMetadata> Metadata { get; set; } = new List<ApiMetadata>();

    public Guid SourceDeviceId { get; set; }
}

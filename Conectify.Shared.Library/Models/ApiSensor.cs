namespace Conectify.Shared.Library.Models;
using System;
using System.Collections.Generic;

public record ApiSensor
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public virtual List<ApiMetadata> Metadata { get; set; } = new List<ApiMetadata>();

    public Guid SourceDeviceId { get; set; }
}

namespace Conectify.Shared.Library.Models;
using System;

public class ApiMetadataConnector
{
    public float? NumericValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public int? TypeValue { get; set; }
    public string Unit { get; set; } = string.Empty;

    public float? MinVal { get; set; }
    public float? MaxVal { get; set; }

    public Guid MetadataId { get; set; }
    public Guid DeviceId { get; set; }
}

using System;

namespace Conectify.Shared.Library.Models;

public class ApiMetadata
{
    public string Name { get; set; } = string.Empty;
    public float? NumericValue { get; set; }
    public string StringValue { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public float? MinVal { get; set; }
    public float? MaxVal { get; set; }

    public Guid MetadataId { get; set; }
}
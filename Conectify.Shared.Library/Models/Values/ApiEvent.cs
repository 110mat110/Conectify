using System;

namespace Conectify.Shared.Library.Models.Values;
public class ApiEvent
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string StringValue { get; set; } = string.Empty;
    public float? NumericValue { get; set; }
    public long TimeCreated { get; set; }
    public Guid SourceId { get; set; }
    public Guid? DestinationId { get; set; }
}

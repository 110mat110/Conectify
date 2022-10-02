namespace Conectify.Services.Automatization.Models;

public class AutomatisationValue
{
    public Guid SourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string StringValue { get; set; } = string.Empty;
    public float? NumericValue { get; set; }
    public long TimeCreated { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? DestinationId { get; set; }
}

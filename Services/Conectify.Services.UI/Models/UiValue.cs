namespace Conectify.Services.UI.Models;

/// <summary>Only the value fields a cube actually renders - no id/type/sourceId fluff.</summary>
public record UiValue
{
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string StringValue { get; init; } = string.Empty;
    public float? NumericValue { get; init; }
    public long TimeCreated { get; init; }
}

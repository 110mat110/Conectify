namespace Conectify.Services.UI.Models;

/// <summary>A colour band for the chart / accent, derived from "Threshold" metadata.</summary>
public record UiThreshold
{
    public float Min { get; init; }
    public float Max { get; init; }
    public string Color { get; init; } = string.Empty;
}

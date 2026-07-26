namespace Conectify.Services.UI.Models;

/// <summary>
/// Everything a sensor cube needs to render - and nothing else. All metadata has already been
/// interpreted by the UI service into visibility, display name, colour thresholds and icons.
/// </summary>
public record UiSensorResponse
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;

    /// <summary>Display-name override from "Name" metadata, or null to fall back to Name @ DeviceName.</summary>
    public string? ShowName { get; init; }
    public bool Visible { get; init; } = true;

    public UiValue? Latest { get; init; }

    /// <summary>Chart series as compact [timeCreated, numericValue] pairs.</summary>
    public IEnumerable<double?[]> Values { get; init; } = [];

    public IEnumerable<UiThreshold> Thresholds { get; init; } = [];
    public IEnumerable<UiIcon> Icons { get; init; } = [];
}

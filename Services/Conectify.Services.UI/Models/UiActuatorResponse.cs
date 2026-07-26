namespace Conectify.Services.UI.Models;

/// <summary>
/// Everything an actuator cube needs to render. The IOType metadata has already been parsed into
/// a numeric control type plus its min/max/value, so the client does no metadata interpretation.
/// </summary>
public record UiActuatorResponse
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public Guid SensorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;

    public string? ShowName { get; init; }
    public bool Visible { get; init; } = true;

    public UiValue? Latest { get; init; }

    /// <summary>Parsed control type: -1 Undefined, 0 Linear, 1 Binary, 2 Trigger, 3 Color, 4 CCT.</summary>
    public int IoType { get; init; } = -1;
    public float MinValue { get; init; }
    public float MaxValue { get; init; }
    public float MetadataValue { get; init; }

    public IEnumerable<UiIcon> Icons { get; init; } = [];
}

namespace Conectify.Services.Android.Models;

/// <summary>
/// A sensor cube exactly as the widget draws it — the accent colour is already resolved from the
/// threshold metadata, so the client does no interpretation and the metadata never crosses the wire.
/// </summary>
public record WidgetSensorDto(
    Guid Id,
    string Name,
    float? NumericValue,
    string StringValue,
    string Unit,
    long TimeCreated,
    string AccentColor
);

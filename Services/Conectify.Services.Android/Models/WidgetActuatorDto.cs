namespace Conectify.Services.Android.Models;

/// <summary>
/// An actuator row exactly as the widget draws it. IOType is already parsed to the numeric form the
/// client switches on, and min/max/trigger are the values its buttons send.
/// </summary>
public record WidgetActuatorDto(
    Guid Id,
    string Name,
    bool IsActive,
    float? CurrentValue,
    string Unit,
    int IoType,
    float MinVal,
    float MaxVal,
    float TriggerValue
);

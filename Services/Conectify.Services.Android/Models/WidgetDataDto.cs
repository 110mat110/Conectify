namespace Conectify.Services.Android.Models;

public record WidgetDataDto(
    List<WidgetSensorDto> Sensors,
    List<WidgetActuatorDto> Actuators
);

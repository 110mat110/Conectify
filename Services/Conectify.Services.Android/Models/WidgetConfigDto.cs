namespace Conectify.Services.Android.Models;

public record WidgetConfigItemDto(Guid DeviceId, string SourceType);

public record AvailableDeviceDto(Guid Id, string Name);

public record AvailableDevicesDto(
    List<AvailableDeviceDto> Sensors,
    List<AvailableDeviceDto> Actuators
);

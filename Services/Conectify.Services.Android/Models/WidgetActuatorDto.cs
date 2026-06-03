namespace Conectify.Services.Android.Models;

public record WidgetActuatorDto(
    Guid Id,
    string Name,
    string SourceDeviceName,
    float? CurrentNumericValue,
    string CurrentStringValue,
    string Unit,
    long LastUpdated,
    List<MetadataDto> Metadata
);

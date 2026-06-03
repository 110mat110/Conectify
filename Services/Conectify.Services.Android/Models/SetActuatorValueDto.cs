namespace Conectify.Services.Android.Models;

public record SetActuatorValueDto(
    float? NumericValue,
    string? StringValue,
    string Unit
);

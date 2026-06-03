namespace Conectify.Services.Android.Models;

public record MetadataDto(
    string Name,
    float? NumericValue,
    string StringValue,
    string Unit,
    float? MinVal,
    float? MaxVal
);

namespace Conectify.Services.Android.Models;

public record CurrentValueDto(
    Guid Id,
    float? NumericValue,
    string StringValue,
    string Unit,
    long TimeCreated
);

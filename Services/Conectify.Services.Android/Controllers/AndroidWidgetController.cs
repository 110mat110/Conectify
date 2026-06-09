using Conectify.Services.Android.Models;
using Conectify.Services.Android.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Android.Controllers;

[ApiController]
[Route("api/android")]
public class AndroidWidgetController(AndroidWidgetService widgetService, ILogger<AndroidWidgetController> logger) : ControllerBase
{
    /// <summary>
    /// Full widget data: all known sensors and actuators with metadata and last value.
    /// Called once on widget load or after manual refresh.
    /// </summary>
    [HttpGet("widget")]
    [ProducesResponseType(typeof(WidgetDataDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWidgetData(
        [FromQuery] string user = "",
        [FromQuery] string widgetType = "large",
        CancellationToken ct = default) =>
        Ok(await widgetService.GetWidgetDataAsync(user, widgetType, ct));

    /// <summary>
    /// Lightweight value refresh: returns only id + current value for each sensor/actuator.
    /// Called periodically (every 10 min) or on manual refresh button.
    /// </summary>
    [HttpGet("values")]
    [ProducesResponseType(typeof(List<CurrentValueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentValues(CancellationToken ct) =>
        Ok(await widgetService.GetCurrentValuesAsync(ct));

    /// <summary>
    /// Sets a new value for an actuator (e.g. button press in the widget).
    /// </summary>
    [HttpPost("actuator/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActuatorValue(Guid id, [FromBody] SetActuatorValueDto dto, CancellationToken ct)
    {
        logger.LogInformation("Widget actuator trigger: actuatorId={ActuatorId} numericValue={NumericValue} stringValue={StringValue} unit={Unit}",
            id, dto.NumericValue, dto.StringValue, dto.Unit);

        var success = await widgetService.SetActuatorValueAsync(id, dto, ct);

        if (!success)
            logger.LogWarning("Widget actuator trigger failed — actuator not found: actuatorId={ActuatorId}", id);

        return success ? NoContent() : NotFound();
    }
}

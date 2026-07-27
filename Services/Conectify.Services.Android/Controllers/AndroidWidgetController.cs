using Conectify.Services.Android.Models;
using Conectify.Services.Android.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Android.Controllers;

[ApiController]
[Route("api/android")]
public class AndroidWidgetController(AndroidWidgetService widgetService, ILogger<AndroidWidgetController> logger) : ControllerBase
{
    /// <summary>
    /// Everything the widget draws, already formatted for it: ordered, filtered to visible devices,
    /// accent colours resolved, IO types parsed, current values attached. Served from memory, so the
    /// widget can call it on every refresh.
    /// </summary>
    [HttpGet("widget")]
    [ProducesResponseType(typeof(WidgetDataDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWidgetData(
        [FromQuery] string user = "",
        [FromQuery] string widgetType = "large",
        CancellationToken ct = default) =>
        Ok(await widgetService.GetWidgetDataAsync(user, widgetType, ct));

    /// <summary>
    /// Sets a new value for an actuator (e.g. button press in the widget).
    /// </summary>
    [HttpPost("actuator/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActuatorValue(Guid id, [FromBody] SetActuatorValueDto dto, CancellationToken ct)
    {
        logger.LogInformation("Widget actuator trigger: actuatorId={ActuatorId} numericValue={NumericValue}",
            id, dto.NumericValue);

        var success = await widgetService.SetActuatorValueAsync(id, dto, ct);

        return success ? NoContent() : NotFound();
    }
}

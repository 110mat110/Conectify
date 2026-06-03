using Conectify.Services.Android.Models;
using Conectify.Services.Android.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conectify.Services.Android.Controllers;

[ApiController]
[Route("api/androidconfig")]
public class AndroidConfigController(AndroidConfigService configService) : ControllerBase
{
    /// <summary>All known sensors and actuators available to add to the widget.</summary>
    [HttpGet("available")]
    public async Task<AvailableDevicesDto> GetAvailable(CancellationToken ct) =>
        await configService.GetAvailableAsync(ct);

    /// <summary>Current widget config for a user and widget type.</summary>
    [HttpGet("{userMail}/{widgetType}")]
    public async Task<List<WidgetConfigItemDto>> GetConfig(
        string userMail, string widgetType, CancellationToken ct) =>
        await configService.GetConfigAsync(userMail, widgetType, ct);

    /// <summary>Save widget config (replaces existing).</summary>
    [HttpPost("{userMail}/{widgetType}")]
    public async Task<IActionResult> SaveConfig(
        string userMail, string widgetType,
        [FromBody] List<WidgetConfigItemDto> items,
        CancellationToken ct)
    {
        await configService.SaveConfigAsync(userMail, widgetType, items, ct);
        return Ok();
    }

    /// <summary>Clear widget config (reverts to dashboard fallback).</summary>
    [HttpDelete("{userMail}/{widgetType}")]
    public async Task<IActionResult> ClearConfig(
        string userMail, string widgetType, CancellationToken ct)
    {
        await configService.SaveConfigAsync(userMail, widgetType, [], ct);
        return Ok();
    }
}

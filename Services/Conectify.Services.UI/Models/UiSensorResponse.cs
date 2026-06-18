using Conectify.Shared.Library.Models;
using Conectify.Shared.Library.Models.Values;

namespace Conectify.Services.UI.Models;

public record UiSensorResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SourceDeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public List<ApiMetadata> Metadata { get; set; } = [];
    public ApiEvent? LatestValue { get; set; }
}

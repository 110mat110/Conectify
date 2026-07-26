using System.Collections.Generic;

namespace Conectify.Shared.Library.Models.Values;

/// <summary>
/// Everything the UI needs for a single source (sensor or actuator sensor) in one payload:
/// the latest value plus the last 24 hours of values. Served purely from the History
/// in-memory cache - never touches the Event table on the request path.
/// </summary>
public class SensorHistoryBundle
{
    public ApiEvent? Latest { get; set; }

    public IEnumerable<ApiEvent> Values { get; set; } = [];
}

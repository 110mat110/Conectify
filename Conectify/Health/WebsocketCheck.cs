using Conectify.Server.Caches;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Conectify.Server.Health;

public class WebsocketCheck(IWebsocketCache websocketCache) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(websocketCache.ActiveSocketCount > 0 ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("0 connected services"));
    }
}

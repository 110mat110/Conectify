using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Conectify.Services.Library;
internal class WebsocketHealthcheck(IServicesWebsocketClient websocketClient) : IHealthCheck
{

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(websocketClient.WebSocketState == System.Net.WebSockets.WebSocketState.Open ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy($"websocket in state {websocketClient.WebSocketState}"));
    }
}

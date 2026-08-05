using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Conectify.Services.Library;
internal class WebsocketHealthcheck(IServicesWebsocketClient websocketClient, IConnectionState connectionState) : IHealthCheck
{

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var socketState = websocketClient.WebSocketState;
        if (socketState != System.Net.WebSockets.WebSocketState.Open)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"websocket in state {socketState}"));
        }

        var data = new Dictionary<string, object>
        {
            ["lastContact"] = connectionState.LastContact,
            ["consecutiveFailures"] = connectionState.ConsecutiveFailures,
        };

        return Task.FromResult(connectionState.Quality switch
        {
            // An open socket used to be enough to report green, which is how a connection that
            // carried nothing all day could stay unnoticed.
            ConnectionQuality.Broken => HealthCheckResult.Unhealthy($"websocket is open but the server is not answering: {connectionState.LastFailure}", data: data),
            ConnectionQuality.Suspect => HealthCheckResult.Degraded($"websocket is open but a check failed: {connectionState.LastFailure}", data: data),
            _ => HealthCheckResult.Healthy("websocket is open and the server is answering", data),
        });
    }
}

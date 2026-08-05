using System.Net.WebSockets;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Conectify.Services.Library;

/// <summary>
/// A websocket can stay in state Open long after the server stopped listening - the connection is
/// then dead but nobody notices, the healthcheck keeps reporting green and no data flows in either
/// direction. This watches the connection end to end instead: once the server goes quiet, it sends a
/// probe addressed to this very device, which the server routes straight back to us. No echo means
/// the connection is only connected on paper, so we throw it away and reconnect, and let the
/// healthcheck report ill. When even that does not help for long enough, the service ends itself so
/// the container starts it again.
/// </summary>
public class ConnectionWatchdog(
    IServicesWebsocketClient websocketClient,
    IConnectionState connectionState,
    IServiceRestarter serviceRestarter,
    ConfigurationBase configuration,
    TimeProvider timeProvider,
    ILogger<ConnectionWatchdog> logger) : BackgroundService
{
    private WatchdogConfiguration Options => configuration.Watchdog;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Options.Enabled)
        {
            logger.LogWarning("Connection watchdog is turned off");
            return;
        }

        logger.LogInformation(
            "Connection watchdog started: checking every {Interval}, probing after {Silence} of silence, restarting after {Restart}",
            Options.CheckInterval,
            Options.SilenceThreshold,
            Options.RestartAfterSeconds > 0 ? Options.RestartAfter : (object)"never");

        try
        {
            await Task.Delay(Options.StartDelay, timeProvider, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunCheckAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Connection watchdog check failed");
                }

                await Task.Delay(Options.CheckInterval, timeProvider, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // the service is shutting down
        }
    }

    internal async Task RunCheckAsync(CancellationToken ct)
    {
        var socketState = websocketClient.WebSocketState;

        if (socketState != WebSocketState.Open)
        {
            await HandleDeadConnectionAsync($"websocket is in state {socketState}", ct);
            return;
        }

        var silence = timeProvider.GetUtcNow() - connectionState.LastContact;
        if (silence < Options.SilenceThreshold)
        {
            // Events are coming in, which is all the proof of life we need - do not bother the server.
            connectionState.ReportSuccessfulCheck();
            return;
        }

        logger.LogInformation("Nothing came from the server for {Silence}, probing the connection", silence);

        if (await ProbeAsync(ct))
        {
            if (connectionState.ConsecutiveFailures > 0)
            {
                logger.LogWarning("Server is answering again after {Failures} failed check(s)", connectionState.ConsecutiveFailures);
            }

            connectionState.ReportSuccessfulCheck();
            return;
        }

        await HandleDeadConnectionAsync($"server did not answer the connection probe in {Options.ProbeTimeout}", ct);
    }

    /// <summary>
    /// Sends an event addressed to our own device. The server delivers events to the device that owns
    /// the destination, so it comes back to us - which proves the connection carries traffic both
    /// ways, unlike the socket state. Anything else the server sends in the meantime counts just as
    /// well, that is the same proof.
    /// </summary>
    private async Task<bool> ProbeAsync(CancellationToken ct)
    {
        var contactsBefore = connectionState.ContactCount;
        var sentAt = timeProvider.GetUtcNow();
        var probe = new WebsocketEvent()
        {
            Id = Guid.NewGuid(),
            Type = Constants.Events.Command,
            Name = Constants.Commands.ConnectionCheck,
            SourceId = configuration.DeviceId,
            DestinationId = configuration.DeviceId,
            NumericValue = 1,
            StringValue = string.Empty,
            Unit = string.Empty,
            TimeCreated = sentAt.ToUnixTimeMilliseconds(),
        };

        if (!await websocketClient.SendMessageAsync(probe, ct))
        {
            logger.LogWarning("Connection probe could not even be sent");
            return false;
        }

        var deadline = sentAt + Options.ProbeTimeout;
        while (true)
        {
            if (connectionState.ContactCount > contactsBefore)
            {
                return true;
            }

            var remaining = deadline - timeProvider.GetUtcNow();
            if (remaining <= TimeSpan.Zero)
            {
                return false;
            }

            await Task.Delay(remaining < Options.ProbePollInterval ? remaining : Options.ProbePollInterval, timeProvider, ct);
        }
    }

    private async Task HandleDeadConnectionAsync(string reason, CancellationToken ct)
    {
        connectionState.ReportFailedCheck(reason);
        logger.LogError(
            "Connection to the server is not working: {Reason} (failed checks in a row: {Failures}, failing since {Since})",
            reason,
            connectionState.ConsecutiveFailures,
            connectionState.FailingSince);

        var failingFor = timeProvider.GetUtcNow() - (connectionState.FailingSince ?? timeProvider.GetUtcNow());
        if (Options.RestartAfterSeconds > 0 && failingFor >= Options.RestartAfter)
        {
            await serviceRestarter.RestartAsync($"connection to the server has been broken for {failingFor} - {reason}");
            return;
        }

        await websocketClient.ForceReconnectAsync(ct);
    }
}

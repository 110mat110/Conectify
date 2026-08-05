using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Conectify.Services.Library;

/// <summary>Last resort of the watchdog - end the service so that whatever runs it starts it again.</summary>
public interface IServiceRestarter
{
    Task RestartAsync(string reason);
}

internal sealed class ServiceRestarter(
    IHostApplicationLifetime lifetime,
    ConfigurationBase configuration,
    ILogger<ServiceRestarter> logger) : IServiceRestarter
{
    private int restartRequested;

    public Task RestartAsync(string reason)
    {
        if (Interlocked.Exchange(ref restartRequested, 1) == 1)
        {
            return Task.CompletedTask;
        }

        logger.LogCritical("Restarting the service: {Reason}", reason);
        lifetime.StopApplication();

        // A clean shutdown is the plan, but the whole reason we are restarting is that something is
        // stuck - so if the host is still up after the grace period, take the process down the hard
        // way. The container restart policy (or systemd) then starts us again with a fresh connection.
        _ = Task.Run(async () =>
        {
            await Task.Delay(configuration.Watchdog.RestartGracePeriod);
            logger.LogCritical("Service did not shut down in {Grace}, killing the process", configuration.Watchdog.RestartGracePeriod);
            Environment.Exit(1);
        });

        return Task.CompletedTask;
    }
}

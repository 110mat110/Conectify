namespace Conectify.Services.Library;

/// <summary>
/// Settings of the connection watchdog, bound from the "Watchdog" section of appsettings. The
/// defaults are meant to be left alone; they are here so a service can loosen them (a service that
/// is quiet by design) or turn the self restart off (a container without a restart policy, where
/// ending the process would leave the service down for good).
/// </summary>
public class WatchdogConfiguration
{
    /// <summary>Turns the whole watchdog off. Only the plain "is the socket open" healthcheck stays.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>How long to leave the service alone after start, so registration and the first connect can finish.</summary>
    public int StartDelaySeconds { get; set; } = 60;

    /// <summary>How often the connection is judged.</summary>
    public int CheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// How long the server may stay quiet before we stop trusting the connection and probe it.
    /// While events are flowing there is nothing to check, so nothing is sent.
    /// </summary>
    public int SilenceThresholdSeconds { get; set; } = 120;

    /// <summary>How long the server has to answer the probe.</summary>
    public int ProbeTimeoutSeconds { get; set; } = 15;

    /// <summary>How many checks in a row have to fail before the healthcheck reports ill.</summary>
    public int FailuresBeforeIll { get; set; } = 2;

    /// <summary>
    /// How long the connection may stay broken - reconnecting all the time and still not working -
    /// before the service takes itself down so the container starts it again. 0 turns the self
    /// restart off and leaves it at reconnecting plus an ill healthcheck.
    /// </summary>
    public int RestartAfterSeconds { get; set; } = 600;

    /// <summary>How long a graceful shutdown gets before the process is killed the hard way.</summary>
    public int RestartGraceSeconds { get; set; } = 30;

    public TimeSpan StartDelay => TimeSpan.FromSeconds(StartDelaySeconds);

    public TimeSpan CheckInterval => TimeSpan.FromSeconds(CheckIntervalSeconds);

    public TimeSpan SilenceThreshold => TimeSpan.FromSeconds(SilenceThresholdSeconds);

    public TimeSpan ProbeTimeout => TimeSpan.FromSeconds(ProbeTimeoutSeconds);

    public TimeSpan RestartAfter => TimeSpan.FromSeconds(RestartAfterSeconds);

    public TimeSpan RestartGracePeriod => TimeSpan.FromSeconds(RestartGraceSeconds);

    /// <summary>How often the probe answer is looked for while waiting for it.</summary>
    public TimeSpan ProbePollInterval => TimeSpan.FromMilliseconds(250);
}

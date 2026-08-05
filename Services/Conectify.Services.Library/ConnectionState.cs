namespace Conectify.Services.Library;

/// <summary>How the watchdog currently judges the connection to the server.</summary>
public enum ConnectionQuality
{
    /// <summary>Events are flowing, or the last probe came back.</summary>
    Healthy,

    /// <summary>A check failed, but not enough times in a row to call the connection dead yet.</summary>
    Suspect,

    /// <summary>The server stopped answering - the service is connected on paper only.</summary>
    Broken,
}

/// <summary>
/// Liveness of the connection, shared by the websocket client (which reports the traffic), the
/// watchdog (which judges it) and the healthcheck (which publishes it). Kept free of dependencies on
/// all three so they can share it without a cycle.
/// </summary>
public interface IConnectionState
{
    ConnectionQuality Quality { get; }

    /// <summary>Last moment anything came in from the server.</summary>
    DateTimeOffset LastContact { get; }

    /// <summary>
    /// How many times anything has come in from the server. The watchdog waits for this to move
    /// rather than for the timestamp, which cannot tell an answer from the moment the probe was sent.
    /// </summary>
    long ContactCount { get; }

    /// <summary>When the current run of failed checks started; null while the connection is fine.</summary>
    DateTimeOffset? FailingSince { get; }

    int ConsecutiveFailures { get; }

    /// <summary>Why the last check failed - for the log and for the healthcheck description.</summary>
    string? LastFailure { get; }

    /// <summary>Reported by the websocket client whenever the server sends something.</summary>
    void ReportContact();

    void ReportSuccessfulCheck();

    void ReportFailedCheck(string reason);
}

internal sealed class ConnectionState(ConfigurationBase configuration, TimeProvider timeProvider) : IConnectionState
{
    private readonly object locker = new();
    private DateTimeOffset lastContact = timeProvider.GetUtcNow();
    private long contactCount;
    private DateTimeOffset? failingSince;
    private int consecutiveFailures;
    private string? lastFailure;

    public ConnectionQuality Quality
    {
        get
        {
            lock (locker)
            {
                if (consecutiveFailures == 0)
                {
                    return ConnectionQuality.Healthy;
                }

                return consecutiveFailures >= Math.Max(1, configuration.Watchdog.FailuresBeforeIll)
                    ? ConnectionQuality.Broken
                    : ConnectionQuality.Suspect;
            }
        }
    }

    public DateTimeOffset LastContact
    {
        get { lock (locker) { return lastContact; } }
    }

    public long ContactCount
    {
        get { lock (locker) { return contactCount; } }
    }

    public DateTimeOffset? FailingSince
    {
        get { lock (locker) { return failingSince; } }
    }

    public int ConsecutiveFailures
    {
        get { lock (locker) { return consecutiveFailures; } }
    }

    public string? LastFailure
    {
        get { lock (locker) { return lastFailure; } }
    }

    public void ReportContact()
    {
        lock (locker)
        {
            lastContact = timeProvider.GetUtcNow();
            contactCount++;
        }
    }

    public void ReportSuccessfulCheck()
    {
        lock (locker)
        {
            consecutiveFailures = 0;
            failingSince = null;
            lastFailure = null;
        }
    }

    public void ReportFailedCheck(string reason)
    {
        lock (locker)
        {
            consecutiveFailures++;
            failingSince ??= timeProvider.GetUtcNow();
            lastFailure = reason;
        }
    }
}

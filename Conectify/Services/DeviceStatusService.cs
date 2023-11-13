using Conectify.Database.Models.Values;
using Conectify.Server.Caches;
using Conectify.Shared.Library;
using System.Timers;

namespace Conectify.Server.Services;

public interface IDeviceStatusService
{
    public Task CheckIfAlive();
}

public class DeviceStatusService : IDeviceStatusService, IDisposable
{
    private readonly IPipelineService pipelineService;
    private readonly ISubscribersCache subscribersCache;
    private readonly Configuration configuration;
    private readonly System.Timers.Timer aTimer;

    public DeviceStatusService(IPipelineService pipelineService, ISubscribersCache subscribersCache, Configuration configuration)
    {
        this.pipelineService = pipelineService;
        this.subscribersCache = subscribersCache;
        this.configuration = configuration;

        // Create a timer with a two second interval.
        aTimer = new System.Timers.Timer(1000 * 60 * 60);
        // Hook up the Elapsed event for the timer. 
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        await CheckIfAlive();
    }

    public async Task CheckIfAlive()
    {
       var allDevices = subscribersCache.AllSubscribers();

       foreach (var device in allDevices.Where(x => x.DeviceId != Guid.Empty).DistinctBy(x => x.DeviceId))
       {
            var command = new Command()
            {
                DestinationId = device.DeviceId,
                Id = Guid.NewGuid(),
                Name = Constants.Commands.ActivityCheck,
                NumericValue = 0,
                SourceId = configuration.DeviceId,
                TimeCreated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Unit = string.Empty,
                StringValue = string.Empty
            };

            await pipelineService.ResendValueToSubscribers(command);
       }
    }

    public void Dispose()
    {
        aTimer.Stop();
        aTimer.Dispose();
    }
}

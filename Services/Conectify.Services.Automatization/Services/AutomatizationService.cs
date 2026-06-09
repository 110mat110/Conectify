using Conectify.Database.Models.Values;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics.Metrics;
using System.Timers;

namespace Conectify.Services.Automatization.Services;

public interface IAutomatizationService
{
    void StartServiceAsync();
}

public class AutomatizationService(IAutomatizationCache automatizationCache,
                             AutomatizationConfiguration configuration,
                             IServicesWebsocketClient websocketClient,
                             IMeterFactory meterFactory,
                             ILogger<AutomatizationService> logger) : IAutomatizationService
{

    private System.Timers.Timer Timer { get; set; } = new System.Timers.Timer(new TimeSpan(0, 0, 1));

    public void StartServiceAsync()
    {
        logger.LogInformation("AutomatizationService starting, refresh interval={IntervalSeconds}s", configuration.RefreshIntervalSeconds);
        websocketClient.OnIncomingEvent += WebsocketClient_OnIncomingEvent;
        websocketClient.ConnectAsync();

        Timer = new System.Timers.Timer(new TimeSpan(0, 0, configuration.RefreshIntervalSeconds));

        Timer.Elapsed += OnTimerElapsed;
        Timer.AutoReset = true;
        Timer.Enabled = true;
        logger.LogInformation("AutomatizationService started");
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        foreach (var rule in await automatizationCache.GetAllRulesAsync())
        {
            rule.Clock(TimeSpan.FromMilliseconds(Timer.Interval));
        }
    }

    private async void WebsocketClient_OnIncomingEvent(Event evnt)
    {
        string name = string.Empty;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await Tracing.Trace(async () =>
        {
            if (evnt.Type == Constants.Events.Action && evnt.DestinationId is not null)
            {
                name = evnt.DestinationId.Value.ToString();
                var sourceRules = await automatizationCache.GetRulesForSourceAsync(evnt.DestinationId.Value);
                logger.LogInformation("Event {EventId} type=Action destinationId={DestinationId} matched {RuleCount} rule(s)",
                    evnt.Id, evnt.DestinationId.Value, sourceRules.Count());
                foreach (var sourceRule in sourceRules)
                {
                    await sourceRule.InsertEvent(evnt, default, meterFactory);
                }
            }
            else
            {
                name = evnt.SourceId.ToString();
                var sourceRules = await automatizationCache.GetRulesForSourceAsync(evnt.SourceId);
                logger.LogInformation("Event {EventId} type={EventType} sourceId={SourceId} matched {RuleCount} rule(s)",
                    evnt.Id, evnt.Type, evnt.SourceId, sourceRules.Count());
                foreach (var sourceRule in sourceRules)
                {
                    await sourceRule.InsertEvent(evnt, default, meterFactory);
                }
            }
        }, evnt.Id, "Rule processing");

        sw.Stop();
        var meter = meterFactory.Create("CustomMeters");
        var allRulesHistogram = meter.CreateHistogram<double>("All_Rules_Processing_Time", "ms");
        allRulesHistogram.Record(sw.Elapsed.TotalMilliseconds);
        var specificRuleHistogram = meter.CreateHistogram<double>(name +"_Rule_Processing_Time", "ms");
        specificRuleHistogram.Record(sw.Elapsed.TotalMilliseconds);
    }
}

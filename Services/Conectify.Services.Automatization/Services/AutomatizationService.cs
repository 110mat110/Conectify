using Conectify.Database.Models.Values;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using System.Diagnostics.Metrics;

namespace Conectify.Services.Automatization.Services;

public interface IAutomatizationService
{
    void StartServiceAsync();
}

public class AutomatizationService(IAutomatizationCache automatizationCache,
                             IRuleScheduler ruleScheduler,
                             IServicesWebsocketClient websocketClient,
                             IMeterFactory meterFactory,
                             ILogger<AutomatizationService> logger) : IAutomatizationService
{
    public void StartServiceAsync()
    {
        logger.LogInformation("AutomatizationService starting");
        websocketClient.OnIncomingEvent += WebsocketClient_OnIncomingEvent;
        websocketClient.ConnectAsync();

        // Time based rules are driven by the scheduler, which sleeps until the closest occurrence is
        // due instead of re-evaluating every rule on a fixed tick.
        ruleScheduler.Start();

        logger.LogInformation("AutomatizationService started");
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

using Conectify.Database.Models.Values;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using System.Timers;

namespace Conectify.Services.Automatization.Services;

public interface IAutomatizationService
{
    void StartServiceAsync();
}

public class AutomatizationService(IAutomatizationCache automatizationCache,
                             AutomatizationConfiguration configuration,
                             IServicesWebsocketClient websocketClient) : IAutomatizationService
{

    private System.Timers.Timer Timer { get; set; } = new System.Timers.Timer(new TimeSpan(0,0,1));

    public void StartServiceAsync()
    {
        websocketClient.OnIncomingEvent += WebsocketClient_OnIncomingEvent;
        websocketClient.ConnectAsync();

        Timer = new System.Timers.Timer(new TimeSpan(0,0,configuration.RefreshIntervalSeconds));

        Timer.Elapsed += OnTimerElapsed;
        Timer.AutoReset = true;
        Timer.Enabled = true;
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
        await Tracing.Trace(async () =>
        {
             if (evnt.Type == Constants.Events.Action && evnt.DestinationId is not null)
             {
                 var sourceRules = await automatizationCache.GetRulesForSourceAsync(evnt.DestinationId.Value);
                 foreach (var sourceRule in sourceRules)
                 {
                     await sourceRule.InsertEvent(evnt, default);
                 }
             }
             else
             {
                 var sourceRules = await automatizationCache.GetRulesForSourceAsync(evnt.SourceId);
                 foreach (var sourceRule in sourceRules)
                 {
                     await sourceRule.InsertEvent(evnt, default);
                 }
             }
         }, evnt.Id, "Rule processing");
    }
}

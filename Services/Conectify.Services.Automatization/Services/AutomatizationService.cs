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

    private System.Timers.Timer timer { get; set; }

    public void StartServiceAsync()
    {
        websocketClient.OnIncomingEvent += WebsocketClient_OnIncomingEvent;
        websocketClient.ConnectAsync();

        timer = new System.Timers.Timer(new TimeSpan(0,0,configuration.RefreshIntervalSeconds));

        timer.Elapsed += OnTimerElapsed;
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        foreach (var rule in await automatizationCache.GetAllRulesAsync())
        {
            rule.Clock(TimeSpan.FromMilliseconds(timer.Interval));
        }
    }

    private async void WebsocketClient_OnIncomingEvent(Event evnt)
    {
        if(evnt.Type == Constants.Events.Action && evnt.DestinationId is not null)
        {
            var sourceRules = automatizationCache.GetRulesForSource(evnt.DestinationId.Value);
            foreach (var sourceRule in sourceRules)
            {
                await sourceRule.InsertEvent(evnt, default);
            }
        } 
        else
        {
            var sourceRules = automatizationCache.GetRulesForSource(evnt.SourceId);
            foreach (var sourceRule in sourceRules)
            {
                await sourceRule.InsertEvent(evnt, default);
            }
        }
    }
}

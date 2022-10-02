using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Library;
using Conectify.Shared.Library.Models.Websocket;

namespace Conectify.Services.Automatization.Services;

public interface IAutomatizationService
{
    Task ExecuteRule(RuleDTO ruleDTO);
    void HandleTimerAsync(CancellationToken cancellationToken = default);
    Task OnValue(Guid ruleId, AutomatisationValue value);

    void StartServiceAsync();
}

public class AutomatizationService : IAutomatizationService
{
    private readonly AutomatizationCache automatizationCache;
    private readonly AutomatizationConfiguration configuration;
    private readonly IServicesWebsocketClient websocketClient;
    private readonly ILogger<AutomatizationService> logger;

    public AutomatizationService(AutomatizationCache automatizationCache,
                                 AutomatizationConfiguration configuration,
                                 IServicesWebsocketClient websocketClient,
                                 ILogger<AutomatizationService> logger)
    {
        this.automatizationCache = automatizationCache;
        this.configuration = configuration;
        this.websocketClient = websocketClient;
        this.logger = logger;
    }

    public void StartServiceAsync()
    {
        websocketClient.OnIncomingValue += WebsocketClient_OnIncomingValue;
        websocketClient.ConnectAsync();
        HandleTimerAsync();
    }

    private async void WebsocketClient_OnIncomingValue(Value value)
    {
        var sourceRules = await automatizationCache.GetRulesForSource(value.SourceId);
        foreach (var sourceRule in sourceRules)
        {
            sourceRule.InsertValue(value);
            await ExecuteRule(sourceRule);
        }
    }

    public async Task OnValue(Guid ruleId, AutomatisationValue value)
    {
        var rule = await automatizationCache.GetRuleById(ruleId);
        if (rule == null)
            return;
        rule.InsertValue(value);
        await ExecuteRule(rule);
    }

    public async Task ExecuteRule(RuleDTO ruleDTO)
    {
        IRuleBehaviour? rule = BehaviourFactory.GetRuleBehaviorByTypeId(ruleDTO.RuleTypeId);
        var result = rule?.Execute(ruleDTO.Values, ruleDTO);
        if(result is null)
        {
            return;
        }

        SendToActuator(ruleDTO, result);

        foreach (RuleDTO nextRule in await automatizationCache.GetNextRules(ruleDTO))
        {
            await OnValue(nextRule.Id, result);
        }
    }

    private void SendToActuator(RuleDTO ruleDTO, AutomatisationValue automatisationValue)
    {
        if (ruleDTO.DestinationActuatorId != null)
        {
            var command = new WebsocketAction()
            {
                DestinationId = ruleDTO.DestinationActuatorId.Value,
                Name = automatisationValue.Name,
                NumericValue = automatisationValue.NumericValue,
                StringValue = automatisationValue.StringValue,
                TimeCreated = automatisationValue.TimeCreated,
                Unit = automatisationValue.Unit,
                SourceId = configuration.SensorId,
            };

            websocketClient.SendMessageAsync(command);
        }
    }

    public async void HandleTimerAsync(CancellationToken cancellationToken = default)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(this.configuration.RefreshIntervalSeconds));

        while (!cancellationToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(cancellationToken);
            logger.LogWarning("Tick in timer handler");
            var rules = await automatizationCache.GetRulesByTypeId(new TimeRuleBehaviour().GetId());

            foreach (var rule in rules)
            {
                await ExecuteRule(rule);
            }
        }
    }
}

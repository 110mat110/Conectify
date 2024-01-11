using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;

namespace Conectify.Services.Automatization.Services;

public interface IAutomatizationService
{
    Task ExecuteRule(RuleDTO ruleDTO);
    Task OnValue(Guid ruleId, AutomatisationValue value);

    void StartServiceAsync();
}

public class AutomatizationService : IAutomatizationService
{
    private readonly AutomatizationCache automatizationCache;
    private readonly AutomatizationConfiguration configuration;
    private readonly IServicesWebsocketClient websocketClient;
    private readonly ITimingService timingService;

    public AutomatizationService(AutomatizationCache automatizationCache,
                                 AutomatizationConfiguration configuration,
                                 IServicesWebsocketClient websocketClient,
                                 ITimingService timingService)
    {
        this.automatizationCache = automatizationCache;
        this.configuration = configuration;
        this.websocketClient = websocketClient;
        this.timingService = timingService;
    }

    public void StartServiceAsync()
    {
        websocketClient.OnIncomingValue += WebsocketClient_OnIncomingValue;
        websocketClient.OnIncomingAction += WebsocketClient_OnIncomingAction;
        websocketClient.ConnectAsync();
        timingService.HandleTimers();
    }

    private async void WebsocketClient_OnIncomingAction(Database.Models.Values.Action action)
    {
        if (action.DestinationId != null)
        {
            var sourceRules = automatizationCache.GetRulesForSource(action.DestinationId.Value);
            foreach (var sourceRule in sourceRules)
            {
                sourceRule.InsertValue(action);
                await ExecuteRule(sourceRule);
            }
        }
    }

    private async void WebsocketClient_OnIncomingValue(Value value)
    {
        var sourceRules = automatizationCache.GetRulesForSource(value.SourceId);
        foreach (var sourceRule in sourceRules)
        {
            sourceRule.InsertValue(value);
            await ExecuteRule(sourceRule);
        }
    }

    public async Task OnValue(Guid ruleId, AutomatisationValue value)
    {
        var rule = await automatizationCache.GetRuleByIdAsync(ruleId);
        if (rule is null)
            return;
        rule.InsertValue(value);
        await ExecuteRule(rule);
    }

    public async Task ExecuteRule(RuleDTO ruleDTO)
    {
        IRuleBehaviour? rule = BehaviourFactory.GetRuleBehaviorByTypeId(ruleDTO.RuleTypeId);
        var parameters = await GetRuleParameterValues(ruleDTO);

        var result = rule?.Execute(ruleDTO.Values, ruleDTO, parameters);
        if (result is null)
        {
            return;
        }
        ruleDTO.OutputValue = result;
        SendToActuator(ruleDTO, result);

        foreach (RuleDTO nextRule in automatizationCache.GetNextRules(ruleDTO))
        {
            await OnValue(nextRule.Id, result);
        }
    }

    private async Task<IEnumerable<Tuple<Guid,AutomatisationValue>>> GetRuleParameterValues(RuleDTO ruleDTO)
    {
        var results = new List<Tuple<Guid,AutomatisationValue>>();
        foreach (var parameter in ruleDTO.Parameters)
        {
            var dto = await automatizationCache.GetRuleByIdAsync(parameter);
            
            if (dto is not null && dto.OutputValue is not null)
            {
                results.Add(new Tuple<Guid, AutomatisationValue>(dto.Id, dto.OutputValue));
            }
        }

        return results;
    }

    private void SendToActuator(RuleDTO ruleDTO, AutomatisationValue automatisationValue)
    {
        if (ruleDTO.DestinationActuatorId != null && ruleDTO.DestinationActuatorId != Guid.Empty)
        {
            var command = new WebsocketBaseModel()
            {
                DestinationId = ruleDTO.DestinationActuatorId.Value,
                Name = automatisationValue.Name,
                NumericValue = automatisationValue.NumericValue,
                StringValue = automatisationValue.StringValue,
                TimeCreated = automatisationValue.TimeCreated,
                Unit = automatisationValue.Unit,
                SourceId = configuration.SensorId,
                Type = Constants.Types.Action,
            };

            websocketClient.SendMessageAsync(command);
        }
    }
}

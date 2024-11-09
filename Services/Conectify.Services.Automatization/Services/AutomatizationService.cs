using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Google.Protobuf.WellKnownTypes;
using System;

namespace Conectify.Services.Automatization.Services;

public interface IAutomatizationService
{
    Task ExecuteRule(RuleDTO ruleDTO);
    Task OnValue(Guid ruleId, AutomatisationValue value);

    void StartServiceAsync();
}

public class AutomatizationService(IAutomatizationCache automatizationCache,
                             AutomatizationConfiguration configuration,
                             IServicesWebsocketClient websocketClient) : IAutomatizationService
{
    public void StartServiceAsync()
    {
        websocketClient.OnIncomingEvent += WebsocketClient_OnIncomingEvent;
        websocketClient.ConnectAsync();
    }

    private async void WebsocketClient_OnIncomingEvent(Event evnt)
    {
        if(evnt.Type == Constants.Events.Action && evnt.DestinationId is not null)
        {
            var sourceRules = automatizationCache.GetRulesForSource(evnt.DestinationId.Value);
            foreach (var sourceRule in sourceRules)
            {
                sourceRule.InsertEvent(evnt);
                await ExecuteRule(sourceRule);
            }
        }
        if(evnt.Type == Constants.Events.Value)
        {
            var sourceRules = automatizationCache.GetRulesForSource(evnt.SourceId);
            foreach (var sourceRule in sourceRules)
            {
                sourceRule.InsertEvent(evnt);
                await ExecuteRule(sourceRule);
            }
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
            var command = new WebsocketEvent()
            {
                DestinationId = ruleDTO.DestinationActuatorId.Value,
                Name = automatisationValue.Name,
                NumericValue = automatisationValue.NumericValue,
                StringValue = automatisationValue.StringValue,
                TimeCreated = automatisationValue.TimeCreated,
                Unit = automatisationValue.Unit,
                SourceId = configuration.SensorId,
                Type = Constants.Events.Action,
            };

            websocketClient.SendMessageAsync(command);
        }
    }
}

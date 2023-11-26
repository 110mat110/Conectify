using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Library;
using Conectify.Shared.Library;
using Conectify.Shared.Library.Models.Websocket;
using Newtonsoft.Json;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace Conectify.Services.Automatization.Services;

public interface IAutomatizationService
{
    Task ExecuteRule(RuleDTO ruleDTO);
    void HandleTimers();
    Task OnValue(Guid ruleId, AutomatisationValue value);

    void StartServiceAsync();
}

public class AutomatizationService : IAutomatizationService
{
    private readonly AutomatizationCache automatizationCache;
    private readonly AutomatizationConfiguration configuration;
    private readonly IServicesWebsocketClient websocketClient;
    private readonly Dictionary<Guid, Timer> timers = new();

    public AutomatizationService(AutomatizationCache automatizationCache,
                                 AutomatizationConfiguration configuration,
                                 IServicesWebsocketClient websocketClient)
    {
        this.automatizationCache = automatizationCache;
        this.configuration = configuration;
        this.websocketClient = websocketClient;
    }

    public void StartServiceAsync()
    {
        websocketClient.OnIncomingValue += WebsocketClient_OnIncomingValue;
        websocketClient.OnIncomingAction += WebsocketClient_OnIncomingAction;
        websocketClient.ConnectAsync();
        HandleTimers();
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

    public void HandleTimers()
    {
        foreach(var timer in timers)
        {
            timer.Value.Dispose();
        }

        timers.Clear();

        var setTimeRules = automatizationCache.GetRulesByTypeId(new TimeRuleBehaviour().GetId());

        foreach (var rule in setTimeRules) {
            CreateTimingFunction(rule);
        }
    }

    private async void ExecuteFunction(object? state)
    {
        if (state is not Guid) return;

        await timers[(Guid)state].DisposeAsync();
        timers.Remove((Guid)state);

        var rule = await automatizationCache.GetRuleByIdAsync((Guid)state);

        if(rule is null) return;

        await ExecuteRule(rule);

        CreateTimingFunction(rule);
    }

    private void CreateTimingFunction(RuleDTO rule)
    {
        var parameters = JsonConvert.DeserializeAnonymousType(rule.ParametersJson, new { TimeSet = "", Days = "" });
        if (parameters is null)
        {
            return;
        }

        var timer = new Timer(ExecuteFunction, rule.Id, CalculateNextExecutionOfRule(parameters.TimeSet, parameters.Days), Timeout.Infinite);

        if (timers.ContainsKey(rule.Id))
        {
            timers[rule.Id].Dispose();
            timers.Remove(rule.Id);
        }

        timers.Add(rule.Id, timer);
    }

    private static long CalculateNextExecutionOfRule(string targetTimeString, string targetDaysOfWeekAbbreviations)
    {
        // Parse the target time string
        
        if (!DateTime.TryParse(targetTimeString, out DateTime targetTime))
        {
            throw new ArgumentException("Invalid target time format");
        }
        targetTime = targetTime.ToUniversalTime();

        // Parse the target day abbreviations
        var targetDaysOfWeek = targetDaysOfWeekAbbreviations.Split(',').Select(abbrev => ParseDayAbbreviation(abbrev)).ToList();

        // Get the current date and time
        DateTime currentTime = DateTime.UtcNow;

        // Calculate the nearest occurrence of the target day of the week
        DateTime nearestOccurrence = CalculateNearestDay(currentTime, targetDaysOfWeek);

        // Set the target date and time
        DateTime targetDateTime = new(nearestOccurrence.Year, nearestOccurrence.Month, nearestOccurrence.Day, targetTime.Hour, targetTime.Minute, targetTime.Second);

        if (targetDateTime < currentTime)
        {
            // If the target time has already passed for today, calculate for the next occurrence on an active day
            nearestOccurrence = CalculateNearestDay(nearestOccurrence.AddDays(1), targetDaysOfWeek);
            targetDateTime = new DateTime(nearestOccurrence.Year, nearestOccurrence.Month, nearestOccurrence.Day, targetTime.Hour, targetTime.Minute, targetTime.Second);
        }

        // Calculate the time difference in milliseconds
        long millisecondsUntilTarget = (long)(targetDateTime - currentTime).TotalMilliseconds;

        return millisecondsUntilTarget;
    }

    private static DayOfWeek ParseDayAbbreviation(string abbreviation)
    {
        return abbreviation.Trim().ToLower() switch
        {
            "mo" => DayOfWeek.Monday,
            "tu" => DayOfWeek.Tuesday,
            "we" => DayOfWeek.Wednesday,
            "th" => DayOfWeek.Thursday,
            "fr" => DayOfWeek.Friday,
            "sa" => DayOfWeek.Saturday,
            "su" => DayOfWeek.Sunday,
            _ => throw new ArgumentException($"Invalid day abbreviation: {abbreviation}"),
        };
    }

    private static DateTime CalculateNearestDay(DateTime currentTime, List<DayOfWeek> targetDaysOfWeek)
    {
        DateTime nearestOccurrence = currentTime;
        bool isNextDay = false;

        foreach (var targetDay in targetDaysOfWeek)
        {
            int daysUntilTargetDay = ((int)targetDay - (int)currentTime.DayOfWeek + 7) % 7;
            DateTime nextTargetDay = currentTime.AddDays(daysUntilTargetDay);

            if (nextTargetDay > nearestOccurrence)
            {
                nearestOccurrence = nextTargetDay;
                isNextDay = true;
            }
        }

        if (!isNextDay)
        {
            nearestOccurrence = CalculateNearestDay(nearestOccurrence.AddDays(1), targetDaysOfWeek);
        }

        return nearestOccurrence;
    }
}

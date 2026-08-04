using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Conectify.Services.Automatization.Services;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

public class SetDelayBehaviour(IServiceProvider serviceProvider) : IRuleBehavior, ISchedulableRuleBehavior
{
    /// <summary>
    /// A delay of zero (or a broken one) would make a rule wired back into itself spin as fast as the
    /// scheduler can dispatch. The old one second timer put the same floor on it implicitly.
    /// </summary>
    private static readonly TimeSpan MinimalDelay = TimeSpan.FromSeconds(1);

    public MinMaxDef Outputs => new(1, 1, 1);

    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(0, 0, 0)),
        new(InputTypeEnum.Trigger, new(1, 1, 1)),
        new(InputTypeEnum.Parameter, new(0, 0, 0))
        ];

    public string DisplayName() => "SET DELAY";

    /// <summary>
    /// Arms the delay. The rule does not fire here, it only writes down when it wants to be woken up
    /// and asks the scheduler to re-plan it.
    /// </summary>
    public Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        var options = ReadOptions(masterRule.ParametersJson);

        if (options is null)
        {
            return Task.CompletedTask;
        }

        var delay = options.Delay > TimeSpan.Zero ? options.Delay : MinimalDelay;

        options.NextTrigger = TimeSource.GetUtcNow().UtcDateTime.Add(delay);
        masterRule.ParametersJson = JsonConvert.SerializeObject(options);

        serviceProvider.GetService<IRuleScheduler>()?.Reschedule(masterRule);

        return Task.CompletedTask;
    }

    /// <summary>
    /// The armed instant, as long as it was not consumed yet. A value already in the past is returned
    /// unchanged so that a delay armed while the scheduler was busy still fires instead of being lost.
    /// </summary>
    public DateTimeOffset? GetNextOccurrence(RuleDTO rule, DateTimeOffset now, DateTimeOffset? lastOccurrence)
    {
        var options = ReadOptions(rule.ParametersJson);

        if (options?.NextTrigger is null)
        {
            return null;
        }

        // NextTrigger is written as UtcNow + delay. Values stored without a kind are treated as UTC,
        // which is how the previous DateTime.UtcNow comparison read them too.
        var next = new DateTimeOffset(DateTime.SpecifyKind(options.NextTrigger.Value, DateTimeKind.Utc), TimeSpan.Zero);

        if (lastOccurrence is not null && next <= lastOccurrence)
        {
            return null;
        }

        return next;
    }

    public async Task OnScheduledAsync(RuleDTO rule, DateTimeOffset occurrence, CancellationToken ct = default)
    {
        var options = ReadOptions(rule.ParametersJson);

        if (options is null)
        {
            return;
        }

        options.NextTrigger = null;
        rule.ParametersJson = JsonConvert.SerializeObject(options);

        await rule.SetAllOutputs(new AutomatisationEvent()
        {
            Name = "DelayRuleResult",
            NumericValue = 0,
            Unit = "",
            TimeCreated = occurrence.ToUnixTimeMilliseconds(),
            SourceId = rule.Id,
            Type = "Trigger",
        });
    }

    public Guid GetId() => Guid.Parse("768fe726-caff-4120-a7f1-3d4c3c6817ac");

    public Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        if (oldDTO is not null)
        {
            rule.ParametersJson = oldDTO.ParametersJson;
        }

        return Task.CompletedTask;
    }

    public Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var options = ReadOptions(rule.ParametersJson);

        if (options?.Delay is not null)
        {
            rule.Description = $"Delay {options.Delay}";
        }

        return Task.CompletedTask;
    }

    private TimeProvider TimeSource => serviceProvider.GetService<TimeProvider>() ?? System.TimeProvider.System;

    private static SetDelayOptions? ReadOptions(string parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<SetDelayOptions>(parametersJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private class SetDelayOptions
    {
        public DateTime? NextTrigger { get; set; }
        public TimeSpan Delay { get; set; }
    }
}

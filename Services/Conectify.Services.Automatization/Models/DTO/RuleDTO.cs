﻿using Conectify.Database.Models.Values;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Rules;
using Conectify.Services.Automatization.Services;
using Conectify.Shared.Library;

namespace Conectify.Services.Automatization.Models.DTO;

public class RuleDTO
{
    public IEnumerable<InputPointDTO> Inputs { get; set; } = [];

    public IEnumerable<OutputPointDTO> Outputs { get; set; } = [];


    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ParametersJson { get; set; } = string.Empty;

    public Guid RuleTypeId { get; set; }

    public Guid? SourceSensorId { get; set; }

    public IRuleBehaviour? RuleBehaviour { get; set; } = null;

    public AutomatisationEvent? OutputValue { get; set; }

    public bool TriggerOnValue { get; set; }

    public void Clock(TimeSpan timeSpan)
    {
        RuleBehaviour?.Clock(this, timeSpan);
    }

    public async Task InsertEvent(Event evnt, CancellationToken ct)
    {
        var automationValue = new AutomatisationEvent()
        {
            Id = evnt.Id,
            Type = evnt.Type,
            Name = evnt.Name,
            NumericValue = evnt.NumericValue,
            StringValue = evnt.StringValue,
            SourceId = evnt.SourceId,
            TimeCreated = evnt.TimeCreated,
            Unit = evnt.Unit,
        };

        await OnTrigger(automationValue, ct);
    }

    public async Task OnTrigger(AutomatisationEvent trigger, CancellationToken ct)
    {
        if (RuleBehaviour is null)
        {
            return;
        }

        await Tracing.Trace(async () => await RuleBehaviour.Execute(this, trigger, ct), trigger.Id, $"Executing rule {RuleBehaviour.DisplayName()}");
    }
    public async Task InitializeAsync(IServiceProvider serviceProvider, RuleDTO? oldDto)
    {
        RuleBehaviour = BehaviourFactory.GetRuleBehaviourByTypeId(RuleTypeId, serviceProvider);

        await Tracing.Trace(async () => await RuleBehaviour.InitializationValue(this, oldDto), Guid.NewGuid(), $"Initializing rule {RuleBehaviour.DisplayName()}");
    }

    public bool CanAddInput(IRuleBehaviour ruleBehaviour, InputTypeEnum inputType)
    {
        return Inputs.Count(x => x.Type == inputType) < ruleBehaviour.Inputs.FirstOrDefault(x => x.Item1 == inputType)?.Item2.Max;
    }

    public bool CanAddOutput(IRuleBehaviour ruleBehaviour)
    {
        return Outputs.Count() < ruleBehaviour.Outputs.Max;
    }

    public async Task SetAllOutputs(AutomatisationEvent evnt, bool trigger = true)
    {
        foreach (var output in Outputs)
        {
            await output.SetOutputEvent(evnt, trigger);
        }
    }

    public async Task<bool> SetAllOutputs(RuleDTO? oldDTO, bool trigger = false)
    {
        if (oldDTO is not null)
        {
            foreach (var output in Outputs)
            {
                var oldOutput = oldDTO.Outputs.FirstOrDefault(x => x.Id == output.Id);
                await output.SetOutputEvent(oldOutput?.Event, trigger);
            }
            return true;
        }

        return false;
    }
}

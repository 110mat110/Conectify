using Conectify.Services.Automatization.Models;
using Conectify.Services.Automatization.Models.ApiModels;
using Conectify.Services.Automatization.Models.Database;
using Conectify.Services.Automatization.Models.DTO;
using Newtonsoft.Json;

namespace Conectify.Services.Automatization.Rules;

/// <summary>
/// Marks behaviours that only exist to serve another feature and should not be offered as a plain
/// new rule in the menu.
/// </summary>
public interface IInternalRuleBehaviour
{
}

/// <summary>
/// A port on the border of a function. It carries whatever arrives on its single input over to its
/// outputs, so that a connection crossing the border of a function is still an ordinary connection
/// between an output point and an input point.
/// </summary>
public abstract class FunctionPortRuleBehaviour(IServiceProvider serviceProvider) : IRuleBehavior, IInternalRuleBehaviour
{
    public MinMaxDef Outputs => new(1, 1, 10);

    /// <summary>
    /// A port carries exactly one signal, its kind is chosen when the port is created. The defaults
    /// are zero because the port is built with the requested input instead of a fixed one.
    /// </summary>
    public IEnumerable<Tuple<InputTypeEnum, MinMaxDef>> Inputs => [
            new(InputTypeEnum.Value, new(0,0,1)),
            new(InputTypeEnum.Trigger, new(0,0,1)),
            new(InputTypeEnum.Parameter, new(0,0,1))
        ];

    public abstract Guid GetId();

    public abstract string DisplayName();

    /// <summary>Wording used in the description, "Input of the function" reads better than a class name.</summary>
    protected abstract string DirectionName { get; }

    public async Task Execute(RuleDTO masterRule, AutomatisationEvent triggerValue, CancellationToken ct = default)
    {
        // A trigger port passes the pulse on. A value port only refreshes what readers behind it pull,
        // triggering there would turn a plain value update into a run of the whole branch.
        var isTrigger = masterRule.Inputs.Any(x => x.Type == InputTypeEnum.Trigger);

        await masterRule.SetAllOutputs(triggerValue, isTrigger);
    }

    public async Task InitializationValue(RuleDTO rule, RuleDTO? oldDTO)
    {
        // Without this a value arriving on the port would be dropped, because a rule only runs on its
        // trigger input by default - and a port has to relay values as well.
        rule.TriggerOnValue = true;

        if (await rule.SetAllOutputs(oldDTO))
        {
            return;
        }

        var input = rule.Inputs.FirstOrDefault();

        if (input is null)
        {
            return;
        }

        var lastValue = await input.GetEvent(serviceProvider);

        if (lastValue is not null)
        {
            await rule.SetAllOutputs(lastValue, false);
        }
    }

    public Task SetParameters(Rule rule, CancellationToken cancellationToken)
    {
        var options = ReadOptions(rule.ParametersJson);
        var name = string.IsNullOrWhiteSpace(options?.Name) ? DisplayName() : options!.Name;

        rule.Name = name;
        rule.Description = $"{DirectionName}: {name}";

        return Task.CompletedTask;
    }

    private static FunctionPortOptions? ReadOptions(string parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<FunctionPortOptions>(parametersJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    internal class FunctionPortOptions
    {
        public string Name { get; set; } = string.Empty;
    }
}

/// <summary>Ids of the port behaviours, so that the function service can look ports up without building one.</summary>
public static class FunctionPortBehaviourIds
{
    public static readonly Guid Input = Guid.Parse("b7e6f0c2-4d1a-4a3e-9f2b-6d5c4b3a2101");

    public static readonly Guid Output = Guid.Parse("b7e6f0c2-4d1a-4a3e-9f2b-6d5c4b3a2102");
}

/// <summary>Brings a signal from the parent board into the function.</summary>
public class FunctionInputRuleBehaviour(IServiceProvider serviceProvider) : FunctionPortRuleBehaviour(serviceProvider)
{
    public override string DisplayName() => "FUNCTION INPUT";

    public override Guid GetId() => FunctionPortBehaviourIds.Input;

    protected override string DirectionName => "Function input";
}

/// <summary>Hands a signal from inside the function back to the parent board.</summary>
public class FunctionOutputRuleBehaviour(IServiceProvider serviceProvider) : FunctionPortRuleBehaviour(serviceProvider)
{
    public override string DisplayName() => "FUNCTION OUTPUT";

    public override Guid GetId() => FunctionPortBehaviourIds.Output;

    protected override string DirectionName => "Function output";
}
